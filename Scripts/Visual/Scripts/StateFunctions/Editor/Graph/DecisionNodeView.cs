﻿using GraphProcessor;
using System;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OneHamsa.Dexterity.Visual
{
	[NodeCustomEditor(typeof(DecisionNode))]
	public class DecisionNodeView : BaseNodeView
	{
		const string kStateNameEmptyMessage = "Empty state name";
		const string kNoInputMessage = "No input(s), node won't run";

		DecisionNode node;

        public override void Enable()
		{
			base.Enable();

			node = nodeTarget as DecisionNode;

			owner.onAfterGraphChanged += HandleGraphChanges;
			HandleGraphChanges(null);

			// runtime
			node.onProcessed += HandleNodeProcessed;
			HandleNodeProcessed();
		}

        public override void Disable()
		{
			base.Disable();

			owner.onAfterGraphChanged -= HandleGraphChanges;
			node.onProcessed -= HandleNodeProcessed;
		}

		private void HandleNodeProcessed()
		{
			EnableInClassList("selected", node.shouldExecute);
		}

		private void HandleGraphChanges(GraphChanges changes)
		{
			HandleStateChange();

			RemoveMessageView(kNoInputMessage);

			if (nodeTarget.inputPorts
				.FirstOrDefault(p => p.fieldName == nameof(DecisionNode.input))
				?.GetEdges()?.Count == 0)
				AddMessageView(kNoInputMessage, NodeMessageType.Warning);
		}

		private void HandleStateChange()
        {
			UpdateTitle();

			RemoveMessageView(kStateNameEmptyMessage);

			if (string.IsNullOrWhiteSpace(node.stateName))
				AddMessageView(kStateNameEmptyMessage, NodeMessageType.Error);
		}
    }
}