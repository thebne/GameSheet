using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneHamsa.Dexterity.Visual
{
    [DefaultExecutionOrder(Manager.modifierExecutionPriority)]
    public abstract class Modifier : TransitionBehaviour, IProvidesStateFunction
    {
        [SerializeField]
        public Node _node;

        [SerializeReference]
        public List<PropertyBase> properties = new List<PropertyBase>();

        public Node node => TryFindNode();

        Dictionary<int, PropertyBase> propertiesCache = null;

        public PropertyBase GetProperty(int stateId)
        {
            // runtime
            if (propertiesCache != null)
            {
                if (!propertiesCache.ContainsKey(stateId))
                {
                    Debug.LogWarning($"property for state = {Manager.instance.GetStateAsString(stateId)} not found", this);
                    // just return first
                    foreach (var p in propertiesCache.Values)
                        return p;
                }
                return propertiesCache[stateId];
            }

            // editor
            foreach (var prop in properties)
                if (Manager.instance.GetStateID(prop.state) == stateId)
                    return prop;

            return null;
        }
        public PropertyBase activeProperty => GetProperty(node.activeState);

        protected virtual void HandleStateChange(int oldState, int newState) { }

        private int[] _states;

        protected override int[] states => _states;
        protected override double currentTime => node.currentTime;
        protected override double stateChangeTime => node.stateChangeTime;
        protected override int activeState => node.activeState;

        StateFunctionGraph IProvidesStateFunction.stateFunctionAsset => node.stateFunctionAsset;

        [Serializable]
        public abstract class PropertyBase
        {
            public string state;
        }

        protected override void Awake()
        {
            propertiesCache = new Dictionary<int, PropertyBase>();
            foreach (var prop in properties)
            {
                var id = Manager.instance.GetStateID(prop.state);
                if (id == -1)
                {
                    // those properties are kept serialized in order to maintain history, no biggie
                    continue;
                }
                propertiesCache.Add(id, prop);
            }
        }

        private void HandleNodeEnabled()
        {
            HandleStateChange(node.activeState, node.activeState);

            _states = new int[propertiesCache.Count];
            var keys = propertiesCache.Keys.GetEnumerator();
            var i = 0;
            while (keys.MoveNext())
                states[i++] = keys.Current;

            InitializeTransitionState();
        }
        protected override void OnEnable()
        {
            if (!EnsureValidState())
            {
                enabled = false;
                return;
            }

            if ((_node = TryFindNode()) == null)
            {
                Debug.LogWarning($"Node not found for modifier ({gameObject.name})");
                enabled = false;
                return;
            }

            if (node.enabled)
                HandleNodeEnabled();
            else
                node.onEnabled += HandleNodeEnabled;

            node.onStateChanged += HandleStateChange;

            base.OnEnable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            node.onEnabled -= HandleNodeEnabled;
            node.onStateChanged -= HandleStateChange;
        }

        Node TryFindNode()
        {
            Node current = _node;
            Transform parent = transform;
            while (current == null && parent != null)
            {
                // include inactive if we're inactive
                if (!gameObject.activeInHierarchy || parent.gameObject.activeInHierarchy)
                    current = parent.GetComponent<Node>();

                parent = parent.parent;
            }

            return current;
        }

        protected bool EnsureValidState()
        {
            if (node == null)
            {
                Debug.LogError("Node is null", this);
                return false;
            }

            if (!node.enabled)
            {
                Debug.LogError("Node is disabled", this);
                return false;
            }

            if (transitionStrategy == null)
            {
                Debug.LogError("No transition strategy assigned", this);
                return false;
            }

            return true;
        }
    }

}
