using SRVR.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace SRVR.Components
{
    public class PickupVacuumable : MonoBehaviour
    {
        public SteamVR_Action_Single grabAction;
        public SteamVR_Action_Skeleton skeletonAction;
        public Transform origin;

        private Vacuumable held;
        private Vacuumable potentialHeld;

        private List<Vacuumable> allPotentialHelds = new List<Vacuumable>();

        private Vector3 lastPos;
        private Vector3 velocity;

        private bool needsExtraFingers = false;
        private int fingersNeeded = 2;

        public void OnEnable() => skeletonAction.onUpdate += OnSkeletonChanged;
        public void OnDisable() => skeletonAction.onUpdate -= OnSkeletonChanged;

        public void ForceDrop()
        {
            if (!held)
                return;

            held.release();
            held.body.velocity = velocity;

            held = null;
            SetHeldRad(0);
        }

        public void Update()
        {
            if (potentialHeld == null && allPotentialHelds.Count > 0)
                potentialHeld = allPotentialHelds[0];
            else if (potentialHeld != null && !allPotentialHelds.Contains(potentialHeld))
                potentialHeld = null;

            if (!held)
                return;

            velocity = (held.transform.position - lastPos) / Time.deltaTime;
            lastPos = held.transform.position;
        }

        public void OnSkeletonChanged(SteamVR_Action_Skeleton fromAction)
        {
            if (held != null)
            {
                if (grabAction.axis >= 0.5f || skeletonAction.GetFingerCurls().Count(x => x >= 0.5) >= 2)
                    return;

                potentialHeld = held;
                held.release();
                held.body.velocity = velocity;

                if (velocity.magnitude >= 3)
                    held.Launch(Vacuumable.LaunchSource.PLAYER);

                held = null;
                SetHeldRad(0);
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
                else if (grabAction.axis < 0.5 && count < 2)
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

                held = potentialHeld;
                potentialHeld = null;

                Patch_Vacuumable.doNotParent = true;
                held.hold();

                SetHeldRad(PhysicsUtil.RadiusOfObject(GameContext.Instance.LookupDirector.GetPrefab(held.identifiable.id)) * 0.646875f);
                held.transform.SetParent(origin, true);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (held)
                return;

            Vacuumable vacuumable = other.GetComponent<Vacuumable>();
            if (vacuumable == null)
                return;

            fingersNeeded = skeletonAction.GetFingerCurls().Count(x => x >= 0.5) + 2;
            needsExtraFingers = fingersNeeded > 2;

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

        private void SetHeldRad(float rad)
        {
            if (rad == 0)
                origin.localPosition = new Vector3(0.646875f, origin.localPosition.y, origin.localPosition.z);
            else
                origin.localPosition = new Vector3(rad, origin.localPosition.y, origin.localPosition.z);
        }
    }
}
