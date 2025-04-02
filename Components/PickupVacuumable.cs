using DG.Tweening;
using SRVR.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using UnityEngine;
using Valve.VR;
using static SECTR_AudioSystem;

namespace SRVR.Components
{
    public class PickupVacuumable : MonoBehaviour
    {
        public SteamVR_Action_Single grabAction;
        public SteamVR_Action_Skeleton skeletonAction;

        public float originScaler = 1f;
        public Transform origin;
        public Transform raycastOrigin;

        public LineRenderer line;

        private Vacuumable held;
        private Vacuumable potentialHeld;
        private bool shouldHaveHeld;

        private List<Vacuumable> allPotentialHelds = new List<Vacuumable>();

        private Vector3 lastPos;
        private Vector3 velocity;

        private bool grabActionNeedsRefresh = false;
        private bool needsExtraFingers = false;
        private int fingersNeeded = 2;

        private bool nonZeroVelocity = true;
        private bool heldIsDistant = false;
        private float moveTime;
        private Vector3 moveStartPos;

        private const float MOVE_LENGTH = 0.2f;
        private const int DISTANCE_MASK = -536887557;

        public void OnEnable()
        {
            skeletonAction.onChange += OnSkeletonChanged;
            grabAction.onChange += OnGrabActionChanged;
        }
        public void OnDisable()
        {
            Drop();

            skeletonAction.onChange -= OnSkeletonChanged;
            grabAction.onChange -= OnGrabActionChanged;
        }

        public void Drop(bool withVelocity = true)
        {
            if (!held)
                return;

            shouldHaveHeld = false;
            HandManager.Instance.heldVacuumables.Remove(held); // done here to prevent infinite loop
            held.release();
            if (withVelocity)
                held.body.velocity = velocity;

            RefreshFingersNeeded();
            nonZeroVelocity = false;
            velocity = Vector3.zero;
            lastPos = Vector3.zero;
            moveTime = 0.0f;
            moveStartPos = Vector3.zero;

            if (velocity.magnitude >= 3)
                held.Launch(Vacuumable.LaunchSource.PLAYER);

            allPotentialHelds.RemoveAll(x => x == null);
            held = null;
            SetHeldRad(0);
        }

        public void Update()
        {
            if (grabActionNeedsRefresh && grabAction.axis < 0.5)
                grabActionNeedsRefresh = false;

            if (held)
            {
                line.enabled = false;
                if (nonZeroVelocity) // this is mostly so that, in the scenario wherein you drop an item while it's distance grab flying towards you, it doesn't go into the atmosphere
                {
                    velocity = (held.transform.position - lastPos) / Time.deltaTime;
                    lastPos = held.transform.position;
                }

                if (moveTime > 0)
                {
                    // using lerp instead of dotween so that it goes to the player hand even if they move their hand
                    held.transform.position = Vector3.Lerp(origin.position, moveStartPos, moveTime / MOVE_LENGTH);
                    
                    moveTime -= Time.deltaTime;
                    if (moveTime <= 0)
                    {
                        held.transform.position = origin.position;
                        moveTime = 0;
                        nonZeroVelocity = true;
                    }
                }
            }
            else
            {
                if (shouldHaveHeld) // make sure it doesn't break if held object gets destroyed
                {
                    shouldHaveHeld = false;

                    allPotentialHelds.RemoveAll(x => x == null);

                    RefreshFingersNeeded();
                    nonZeroVelocity = false;
                    velocity = Vector3.zero;
                    lastPos = Vector3.zero;
                    moveTime = 0.0f;
                    moveStartPos = Vector3.zero;

                    held = null;
                    SetHeldRad(0);
                }

                // if NOTHING is in the hand trigger, distance grab may commence
                // if there's no potential held currently, but there can be, set it to that
                // if potential held is not a valid potential held, set it to null (prevents stale references)
                if (allPotentialHelds.Count == 0)
                {
                    if (VRConfig.DISTANCE_GRAB && Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out RaycastHit hit, 3f, DISTANCE_MASK, QueryTriggerInteraction.Ignore))
                    {
                        heldIsDistant = true;
                        potentialHeld = hit.collider.GetComponent<Vacuumable>();
                    }
                    else
                    {
                        heldIsDistant = false;
                        potentialHeld = null;
                    }
                }
                else if (potentialHeld == null)
                {
                    heldIsDistant = false;
                    potentialHeld = allPotentialHelds[0];
                }
                else if (!allPotentialHelds.Contains(potentialHeld))
                {
                    heldIsDistant = false;
                    potentialHeld = null;
                }

                if (potentialHeld == null)
                {
                    line.enabled = false;
                    return;
                }

                line.enabled = true;
                line.SetPositions(new[] { raycastOrigin.position, potentialHeld.transform.position });
            }
        }

        private void OnGrabActionChanged(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) => OnActionsChanged();
        private void OnSkeletonChanged(SteamVR_Action_Skeleton fromAction) => OnActionsChanged();

        private void OnActionsChanged()
        {
            if (held != null)
            {
                if (grabAction.axis >= 0.5f || skeletonAction.GetFingerCurls().Count(x => x >= 0.5) >= 2)
                    return;

                Drop(nonZeroVelocity);
            }
            else
            {
                if (potentialHeld == null)
                    return;

                float[] fingerCurls = skeletonAction.GetFingerCurls();
                int count = fingerCurls.Count(x => x >= 0.5);

                if (needsExtraFingers)
                {
                    // if more fingers put up, reduce the needed count down to a min of 2
                    // after that, if the min is normal, stop needing extra fingers
                    // if neither is the case, and the needs extra fingers requirement is fulfilled, stop needing
                    // if none of the above, skip holding
                    // nightmare
                    if (count < (fingersNeeded - 2))
                        fingersNeeded = Mathf.Max(count + 2, 2);
                    if (fingersNeeded <= 2)
                    {
                        needsExtraFingers = false;
                        fingersNeeded = 2;
                        return;
                    }
                    else if (count >= fingersNeeded)
                    {
                        needsExtraFingers = false;
                        fingersNeeded = 2;
                    }
                    else return;
                }
                else if ((grabAction.axis < 0.5 || grabActionNeedsRefresh) && count < 2)
                    return;

                // pick objects off of treeeeees
                ResourceCycle heldResource = potentialHeld.GetComponentInChildren<ResourceCycle>();
                if (heldResource)
                {
                    if (heldResource.model.state >= ResourceCycle.State.RIPE)
                        heldResource.DetachFromJoint();
                    else
                        return;
                }

                potentialHeld.GetComponentInChildren<FashionPod>()?.fashionJoint?.Destroy();

                shouldHaveHeld = true;
                held = potentialHeld;
                potentialHeld = null;

                if (!HandManager.Instance.heldVacuumables.ContainsKey(held) && held.isHeld())
                    HandManager.Instance.vacuumer.DropAllVacced();
                held.release();
                Patch_Vacuumable.doNotParent = true;
                held.hold();
                HandManager.Instance.heldVacuumables[held] = this;

                float radius = PhysicsUtil.RadiusOfObject(GameContext.Instance.LookupDirector.GetPrefab(held.identifiable.id)) * originScaler;
                SetHeldRad(radius);
                held.transform.SetParent(origin, true);

                nonZeroVelocity = !heldIsDistant;
                velocity = Vector3.zero;
                lastPos = held.transform.position;

                // if object is close enough, no reason to snap to origin, it just makes it weird
                if (heldIsDistant && Vector3.Distance(transform.position, held.transform.position) > radius + 0.1)
                {
                    moveTime = MOVE_LENGTH;
                    moveStartPos = held.transform.position;
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (held)
                return;

            Vacuumable vacuumable = other.GetComponent<Vacuumable>();
            if (vacuumable == null)
                return;

            RefreshFingersNeeded();

            potentialHeld = vacuumable;
            allPotentialHelds.Add(vacuumable);
        }

        public void OnTriggerExit(Collider other)
        {
            Vacuumable vacuumable = other.GetComponent<Vacuumable>();
            if (vacuumable)
            {
                allPotentialHelds.Remove(vacuumable);

                if (vacuumable == potentialHeld)
                    potentialHeld = null;
            }
        }

        private void RefreshFingersNeeded()
        {
            // ensure that you don't just magnetize items if you already have fingers/action down
            fingersNeeded = skeletonAction.GetFingerCurls().Count(x => x >= 0.5) + 2;
            needsExtraFingers = fingersNeeded > 2;
            grabActionNeedsRefresh = grabAction.axis > 0.5f;
        }

        private void SetHeldRad(float rad) // tries and usually fails to prevent floating
        {
            if (rad == 0)
                origin.localPosition = new Vector3(originScaler, origin.localPosition.y, origin.localPosition.z);
            else
                origin.localPosition = new Vector3(rad, origin.localPosition.y, origin.localPosition.z);
        }
    }
}
