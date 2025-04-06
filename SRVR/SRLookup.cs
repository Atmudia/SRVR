using System.Collections.Generic;
 using System.Linq;
 using Object = UnityEngine.Object;
 using UnityEngine;
 using System;
 using UnityEngine.Experimental.Rendering;

 namespace SRVR
 {
     public static class SRLookup
     {
         private static readonly Dictionary<Type, Object[]> cache = new Dictionary<Type, Object[]>();
 
         public static T Get<T>(string name) where T : Object
         {
             Type selected = typeof(T);
             if (!cache.ContainsKey(selected))
                 cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());
 
             T found = (T)cache[selected].FirstOrDefault(x => x.name == name);
             if (found == null)
             {
                 cache[selected] = Resources.FindObjectsOfTypeAll<T>();
                 found = (T)cache[selected].FirstOrDefault(x => x.name == name);
             }
 
             return found;
         }
         public static T GetCopy<T>(string name) where T : Object =>
             Object.Instantiate(Get<T>(name));
 
         public static T Get<T>(string name, System.Func<T, bool> predicate) where T : Object
         {
             Type selected = typeof(T);
             if (!cache.ContainsKey(selected))
                 cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());
 
             T found = (T)cache[selected].FirstOrDefault(x => x.name == name && predicate((T)x));
             if (found == null)
             {
                 cache[selected] = Resources.FindObjectsOfTypeAll<T>();
                 found = (T)cache[selected].FirstOrDefault(x => x.name == name && predicate((T)x));
             }
 
             return found;
         }
 
         public static T[] GetAll<T>(string name) where T : Object
         {
             Type selected = typeof(T);
             if (!cache.ContainsKey(selected))
                 cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());
 
             T[] found = cache[selected].Where(x => x.name == name).Select(y => (T)y).ToArray();
             if (found.Length == 0)
             {
                 cache[selected] = Resources.FindObjectsOfTypeAll<T>();
                 found = cache[selected].Where(x => x.name == name).Select(y => (T)y).ToArray();
             }
 
             return found;
         }
         public static Texture2D GetReadable(this Texture2D source, TextureFormat format = TextureFormat.ARGB32)
         {
             var prev = RenderTexture.active;
             var temp = RenderTexture.GetTemporary(new RenderTextureDescriptor(source.width, source.height, GraphicsFormatUtility.GetGraphicsFormat(format, GraphicsFormatUtility.IsSRGBFormat(source.graphicsFormat)), 16, source.mipmapCount));
             temp.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
             temp.useMipMap = true;
             Graphics.Blit(source, temp);
             Graphics.SetRenderTarget(temp,0);
             Texture2D texture = new(source.width, source.height, format, source.mipmapCount, !GraphicsFormatUtility.IsSRGBFormat(source.graphicsFormat));
             texture.ReadPixels(new(0, 0, source.width, source.height), 0, 0, true);
             texture.Apply(true, false);
             RenderTexture.active = prev;
             RenderTexture.ReleaseTemporary(temp);
             return texture;
         }
         public static Texture2D MergeSpritesWithPivots(params Sprite[] spriteArray)
         {
             if (spriteArray.Length == 0) return null;
             
             int totalWidth = 0;
             int maxHeight = 0;

             foreach (Sprite sprite in spriteArray)
             {
                 totalWidth += (int)sprite.rect.width;
                 maxHeight = Mathf.Max(maxHeight, (int)sprite.rect.height);
             }

             Texture2D result = new Texture2D(totalWidth, maxHeight, TextureFormat.RGBA32, false);
        
             int offsetX = 0;
             foreach (Sprite sprite in spriteArray)
             {
                 Texture2D tex = sprite.texture.GetReadable();

                 Rect rect = sprite.rect;
                 Color[] pixels = tex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

                 result.SetPixels(offsetX, maxHeight - (int)rect.height, (int)rect.width, (int)rect.height, pixels);
                 offsetX += (int)rect.width;
             }

             result.Apply();
             return result;
         }
         public static Sprite CreateSprite(this Texture2D texture)
         {
             var s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);
             s.name = texture.name;
             return s;
         }
     }
 }