﻿namespace LanguageCompiler.Nodes.Statements
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Irony.Parsing;
    using LanguageCompiler.Errors;
    using LanguageCompiler.Nodes.ClassMembers;
    using LanguageCompiler.Nodes.Statements.CommandStatements;
    using LanguageCompiler.Nodes.Statements.ControlStatements;
    using LanguageCompiler.Semantics;

    /// <summary>
    /// Holds all data related to a "Block" rule.
    /// </summary>
    public class Block : BaseNode
    {
        /// <summary>
        /// Statements inside this block.
        /// </summary>
        private List<BaseNode> statements = new List<BaseNode>();

        /// <summary>
        /// Gets the list of statements held by the block.
        /// </summary>
        public List<BaseNode> Statements
        {
            get { return this.statements; }
        }

        /// <summary>
        /// Forms a valid tree node representing this object.
        /// </summary>
        /// <returns>The formed tree node.</returns>
        public override TreeNode GetGUINode()
        {
            TreeNode result = new TreeNode("Block: Count = " + this.statements.Count);
            foreach (BaseNode child in this.statements)
            {
                result.Nodes.Add(child.GetGUINode());
            }

            return result;
        }

        /// <summary>
        /// Recieves an irony ParseTreeNode and constructs its contents.
        /// </summary>
        /// <param name="node">The irony ParseTreeNode.</param>
        public override void RecieveData(ParseTreeNode node)
        {
            foreach (ParseTreeNode statement in node.ChildNodes[1].ChildNodes)
            {
                if (statement.Term.Name == LanguageGrammar.ContinueStatement.Name)
                {
                    this.statements.Add(new ContinueStatement(
                        statement.ChildNodes[0].Token.Location,
                        statement.ChildNodes[statement.ChildNodes.Count - 1].Token.Location));
                }
                else if (statement.Term.Name == LanguageGrammar.BreakStatement.Name)
                {
                    this.statements.Add(new BreakStatement(
                        statement.ChildNodes[0].Token.Location,
                        statement.ChildNodes[statement.ChildNodes.Count - 1].Token.Location));
                }
                else
                {
                    BaseNode statementNode = null;
                    if (statement.Term.Name == LanguageGrammar.ReturnStatement.Name)
                    {
                        statementNode = new ReturnStatement();
                    }
                    else if (statement.Term.Name == LanguageGrammar.DeclarationStatement.Name)
                    {
                        statementNode = new DeclarationStatement();
                    }
                    else if (statement.Term.Name == LanguageGrammar.Block.Name)
                    {
                        statementNode = new Block();
                    }
                    else if (statement.Term.Name == LanguageGrammar.WhileStatement.Name)
                    {
                        statementNode = new WhileStatement();
                    }
                    else if (statement.Term.Name == LanguageGrammar.DoWhileStatement.Name)
                    {
                        statementNode = new DoWhileStatement();
                    }
                    else if (statement.Term.Name == LanguageGrammar.IfStatement.Name)
                    {
                        statementNode = new IfStatement();
                    }
                    else if (statement.Term.Name == LanguageGrammar.ForStatement.Name)
                    {
                        statementNode = new ForStatement();
                    }
                    else if (statement.Term.Name == LanguageGrammar.ExpressionStatement.Name)
                    {
                        statementNode = new ExpressionStatement();
                    }

                    statementNode.RecieveData(statement);
                    this.statements.Add(statementNode);
                }
            }

            this.StartLocation = node.ChildNodes[0].Token.Location;
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
            scopeStack.AddLevel(ScopeType.Block, this);
            bool foundReturn = false;

            foreach (BaseNode child in this.statements)
            {
                if (foundReturn)
                {
                    CompilerService.Instance.Errors.Add(ErrorsFactory.SemanticError(ErrorType.UnreachableCodeDetected, child));
                    foundErrors = true;
                    break;
                }

                if (child.ReturnsAValue())
                {
                    foundReturn = true;
                }

                foundErrors |= child.CheckSemanticErrors(scopeStack);
                if (foundErrors)
                {
                    break;
                }
            }

            scopeStack.DeleteLevel();
            return foundErrors;
        }

        /// <summary>
        /// Checks if a statement or block of code returns a value.
        /// </summary>
        /// <returns>True if it returns a value, false otherwise.</returns>
        public override bool ReturnsAValue()
        {
            foreach (BaseNode statement in this.statements)
            {
                if (statement.ReturnsAValue())
                {
                    return true;
                }
            }

            return false;
        }
    }
}