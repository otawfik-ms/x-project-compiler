﻿namespace LanguageCompiler.Nodes.ClassMembers
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Irony.Parsing;
    using LanguageCompiler.Errors;
    using LanguageCompiler.Nodes.TopLevel;
    using LanguageCompiler.Semantics;

    /// <summary>
    /// Holds all data related to a "Field Definition" rule.
    /// </summary>
    public class FieldDefinition : MemberDefinition
    {
        /// <summary>
        /// Atoms of this field.
        /// </summary>
        private List<FieldAtom> atoms = new List<FieldAtom>();

        /// <summary>
        /// Initializes a new instance of the FieldDefinition class.
        /// </summary>
        /// <param name="parent">The class where this member was defined in.</param>
        public FieldDefinition(ClassDefinition parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Gets the list of field atoms.
        /// </summary>
        public List<FieldAtom> Atoms
        {
            get { return this.atoms; }
        }

        /// <summary>
        /// Forms a valid tree node representing this object.
        /// </summary>
        /// <returns>The formed tree node.</returns>
        public override TreeNode GetGUINode()
        {
            TreeNode result = base.GetGUINode();
            result.Text = "Field Declaration";

            TreeNode atoms = new TreeNode("Atoms: Count = " + this.atoms.Count);
            foreach (FieldAtom atom in this.atoms)
            {
                atoms.Nodes.Add(atom.GetGUINode());
            }

            result.Nodes.Add(atoms);
            return result;
        }

        /// <summary>
        /// Recieves an irony ParseTreeNode and constructs its contents.
        /// </summary>
        /// <param name="node">The irony ParseTreeNode.</param>
        public override void RecieveData(ParseTreeNode node)
        {
            base.RecieveData(node);
            foreach (ParseTreeNode child in node.ChildNodes[4].ChildNodes)
            {
                FieldAtom atom = new FieldAtom();
                atom.RecieveData(child);
                this.atoms.Add(atom);
            }

            this.EndLocation = node.ChildNodes[node.ChildNodes.Count - 1].Token.Location;
        }

        /// <summary>
        /// Checks for semantic errors within this node.
        /// </summary>
        /// <param name="scopeStack">The scope stack associated with this node.</param>
        /// <returns>True if errors are found, false otherwise.</returns>
        public override bool CheckSemanticErrors(ScopeStack scopeStack)
        {
            bool foundErrors = false;
            if (this.ModifierType != MemberModifierType.Normal)
            {
                this.AddError(ErrorType.FieldInvalidModifier);
                return true;
            }

            foreach (FieldAtom atom in this.atoms)
            {
                foundErrors |= atom.CheckSemanticErrors(scopeStack);
                if (foundErrors)
                {
                    break;
                }

                if (atom.Value != null)
                {
                    if (this.Type.GetExpressionType(scopeStack).IsEqualTo(atom.Value.GetExpressionType(scopeStack)) == false)
                    {
                        this.AddError(ErrorType.ExpressionDoesnotMatchType);
                        foundErrors = true;
                    }
                }
            }

            return foundErrors;
        }

        /// <summary>
        /// Checks if a statement or block of code returns a value.
        /// </summary>
        /// <returns>True if it returns a value, false otherwise.</returns>
        public override bool ReturnsAValue()
        {
            return false;
        }
    }
}