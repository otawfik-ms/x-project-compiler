﻿namespace LanguageCompiler.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using LanguageCompiler.Errors;
    using LanguageCompiler.Nodes;
    using LanguageCompiler.Nodes.ClassMembers;
    using LanguageCompiler.Nodes.TopLevel;

    /// <summary>
    /// A stack of scopes (used in semantic checking).
    /// </summary>
    public class ScopeStack
    {
        /// <summary>
        /// Holds all variables defined within a stack of scopes.
        /// </summary>
        private Stack<Scope> stack = new Stack<Scope>();
        
        /// <summary>
        /// Declares a variable in this scope.
        /// </summary>
        /// <param name="v">Variable to be declared.</param>
        /// <param name="parent">Parent of this variable.</param>
        /// <returns>True if declaration is successful, false otherwise.</returns>
        public bool DeclareVariable(Variable v, BaseNode parent)
        {
            if (this.Containes(v) == false)
            {
                this.stack.Peek().Variables.Add(v);
                return true;
            }
            else
            {
                CompilerService.Instance.Errors.Add(ErrorsFactory.SemanticError(
                    ErrorType.ItemAlreadyDefined,
                    parent,
                    v.Name));
                return false;
            }
        }

        /// <summary>
        /// Adds a new level to the stack.
        /// </summary>
        /// <param name="type">ScopeType to be added.</param>
        /// <param name="node">The node this scope was defined in.</param>
        public void AddLevel(ScopeType type, BaseNode node)
        {
            this.stack.Push(new Scope(type, node));
        }

        /// <summary>
        /// Deletes the last level added to the stack.
        /// </summary>
        public void DeleteLevel()
        {
            this.stack.Pop();
        }

        /// <summary>
        /// Checks if this stack already containes a variable.
        /// </summary>
        /// <param name="v">Variable to be checked.</param>
        /// <returns>True if the variable exists in the stack, false otherwise.</returns>
        public bool Containes(Variable v)
        {
            foreach (Scope scope in this.stack)
            {
                if (scope.Variables.Any(x => x.Name == v.Name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if this stack already containes a variable.
        /// </summary>
        /// <param name="variableName">Variable's name to be checked.</param>
        /// <returns>True if the variable exists in the stack, false otherwise.</returns>
        public bool Containes(string variableName)
        {
            foreach (Scope scope in this.stack)
            {
                if (scope.Variables.Any(v => v.Name == variableName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a variable from the scope stack.
        /// </summary>
        /// <param name="name">Name of this variable.</param>
        /// <returns>The variable found, or null if not found.</returns>
        public Variable GetVariable(string name)
        {
            foreach (Scope scope in this.stack)
            {
                foreach (Variable variable in scope.Variables)
                {
                    if (variable.Name == name)
                    {
                        return variable;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if this type exists in the scope stack.
        /// </summary>
        /// <param name="type">Type to be checked.</param>
        /// <returns>True if type was found, false otherwise.</returns>
        public bool CheckParentScopes(ScopeType type)
        {
            foreach (Scope scope in this.stack)
            {
                if (scope.Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the function or operator in this scope.
        /// </summary>
        /// <returns>A MemberDefinition object.</returns>
        public MemberDefinition GetFunction()
        {
            foreach (Scope scope in this.stack)
            {
                if (scope.Type == ScopeType.Function)
                {
                    return scope.Node as MemberDefinition;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the class existing in this scope.
        /// </summary>
        /// <returns>A ClassDefinition object.</returns>
        public ClassDefinition GetClass()
        {
            foreach (Scope scope in this.stack)
            {
                if (scope.Type == ScopeType.Class)
                {
                    return scope.Node as ClassDefinition;
                }
            }

            return null;
        }
    }
}