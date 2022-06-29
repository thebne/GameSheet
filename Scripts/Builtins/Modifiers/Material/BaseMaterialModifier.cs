using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHamsa.Dexterity.Visual
{
    /// <summary>
    /// Base class for material modifiers. Takes care of setting up editor transitions
    /// </summary>
    public abstract class BaseMaterialModifier : Modifier
    {
        protected struct SupportedComponentActions {
            public Func<Component, (Material material, bool created)> getMaterial;
            public Func<Component, Material> getSharedMaterial;
            public Action<Component, Material> setSharedMaterial;
        }

        private static Dictionary<Type, SupportedComponentActions> supportedComponents = new();
        
        static void AddSupportedComponent<T>(Func<T, (Material material, bool created)> getMaterial, Func<T, Material> getSharedMaterial,
            Action<T, Material> setSharedMaterial) where T : Component
        {
            supportedComponents.Add(typeof(T), new SupportedComponentActions {
                getMaterial = (c) => getMaterial((T)c),
                getSharedMaterial = (c) => getSharedMaterial((T)c),
                setSharedMaterial =  (c, material) => setSharedMaterial((T)c, material),
            });
        }

        static BaseMaterialModifier()
        {
            AddSupportedComponent<Renderer>(
                c => (c.material, false), 
                c => c.sharedMaterial, 
                (c, m) => c.sharedMaterial = m
                );
            AddSupportedComponent<TextMeshProUGUI>(
                c => (c.fontMaterial, false), 
                c => c.fontSharedMaterial,
                (c, m) => c.fontSharedMaterial = m
                );
            AddSupportedComponent<Image>(
                // for image component, material is actually sharedMaterial (it won't create a new one for us)
                c =>
                {
                    var material = new Material(c.material);
                    c.material = material;
                    return (material, true);
                }, 
                c => c.material,
                (c, m) => c.material = m);
        }
        
        private Material originalMaterial;
        protected Material targetMaterial;
        private (Component component, SupportedComponentActions actions) _cached;
        private bool shouldDestroyTargetMaterial;

        protected Component component {
            get {
                if (_cached.component == null)
                    CacheComponent();
                return _cached.component;
            }
        }
        protected SupportedComponentActions actions {
            get {
                if (_cached.component == null)
                    CacheComponent();
                return _cached.actions;
            }
        }

        public override void Awake()
        {
            base.Awake();

            #if UNITY_EDITOR
            // support editor transitions
            if (!Application.isPlaying && targetMaterial == null)
            {
                originalMaterial = actions.getSharedMaterial(component);
                (targetMaterial, shouldDestroyTargetMaterial) = (new Material(originalMaterial), true);
                targetMaterial.EnableKeyword("_NORMALMAP");
                targetMaterial.EnableKeyword("_DETAIL_MULX2");
                actions.setSharedMaterial(component, targetMaterial);
            }
            else
            {
                (targetMaterial, shouldDestroyTargetMaterial) = actions.getMaterial(component);
            }
            #else
            (targetMaterial, shouldDestroyTargetMaterial) = actions.getMaterial(component);
            #endif
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                actions.setSharedMaterial(component, originalMaterial);
            #endif

            if (targetMaterial != null && shouldDestroyTargetMaterial)
            {
                if (!Application.isPlaying)
                    DestroyImmediate(targetMaterial);
                else
                    Destroy(targetMaterial);
                
                targetMaterial = null;
            }
        }
        

        private void CacheComponent()
        {
            foreach (var kv in supportedComponents)
            {
                var t = kv.Key;
                if (GetComponent(t) != null)
                {
                    var component = GetComponent(t);
                    _cached = (component, kv.Value);
                    return;
                }
            }
            Debug.LogError("No supported component found for ColorModifier", this);
            if (Application.isPlaying)
                enabled = false;
        }

        protected void Start()
        {
            CacheComponent();
        }
    }
}
