﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using System.IO;

namespace Microsoft.Cci.Extensions.Experimental
{
#pragma warning disable 612,618
    internal class APIEmitter : BaseMetadataTraverser
#pragma warning restore 612,618
    {
        private TextWriter _writer;
        private int _indentLevel;

        public void EmitAssembly(IAssembly assembly)
        {
            _writer = Console.Out;

            Visit(assembly);
        }

        public override void Visit(IAssembly assembly)
        {
            Visit(assembly.NamespaceRoot);
        }

        public override void Visit(INamespaceDefinition @namespace)
        {
            IEnumerable<INamespaceDefinition> namespaces = @namespace.Members.OfType<INamespaceDefinition>();
            IEnumerable<INamespaceTypeDefinition> types = @namespace.Members.OfType<INamespaceTypeDefinition>();

            if (types.Count() > 0)
            {
                EmitKeyword("namespace");
                Emit(TypeHelper.GetNamespaceName((IUnitNamespaceReference)@namespace, NameFormattingOptions.None));
                EmitNewLine();
                using (EmitBlock(true))
                {
                    foreach (var type in types)
                        Visit(type);
                }
                EmitNewLine();
            }

            foreach(var nestedNamespace in namespaces)
                Visit(nestedNamespace);
        }

        public override void Visit(INamespaceTypeDefinition type)
        {
            EmitType(type, type.Name.Value);
        }

        public override void Visit(INestedTypeDefinition nestedType)
        {
            EmitType(nestedType, nestedType.Name.Value);
        }

        public virtual void EmitType(ITypeDefinition type, string typeName)
        {
            EmitVisibility(type.GetVisibility());
            EmitKeyword("class");
            Emit(typeName);
            EmitNewLine();
            using (EmitBlock(true))
            {
                foreach (var member in type.Members)
                    Visit(member);
            }
            EmitNewLine();

            foreach (var nestedType in type.NestedTypes)
                Visit(nestedType);
        }

        public override void Visit(IFieldDefinition field)
        {
            Emit(MemberHelper.GetMemberSignature(field, NameFormattingOptions.Signature));
            EmitNewLine();
        }

        public override void Visit(IMethodDefinition method)
        {
            Emit(MemberHelper.GetMemberSignature(method, NameFormattingOptions.Signature));
            EmitNewLine();
        }

        public override void Visit(IPropertyDefinition property)
        {
            Emit(MemberHelper.GetMemberSignature(property, NameFormattingOptions.Signature));
            EmitNewLine();
        }

        public override void Visit(IEventDefinition @event)
        {
            Emit(MemberHelper.GetMemberSignature(@event, NameFormattingOptions.Signature));
            EmitNewLine();
        }

        public virtual IDisposable EmitBlock(bool ident)
        {
            return new CodeBlock(this, ident);
        }

        public virtual void EmitBlockStart(bool ident)
        {
            Emit("{");
            if (ident)
            {
                this._indentLevel++;
                EmitNewLine();
            }
        }

        public virtual void EmitBlockEnd(bool ident)
        {
            if (ident)
            {
                this._indentLevel--;
                EmitNewLine();
            }
            Emit("}");
        }

        public virtual void EmitVisibility(TypeMemberVisibility visibility)
        {
            switch (visibility)
            {
                case TypeMemberVisibility.Public:
                    EmitKeyword("public");
                    break;

                case TypeMemberVisibility.Private:
                    EmitKeyword("private");
                    break;

                case TypeMemberVisibility.Assembly:
                    EmitKeyword("internal");
                    break;

                case TypeMemberVisibility.Family:
                    EmitKeyword("protected");
                    break;

                case TypeMemberVisibility.FamilyOrAssembly:
                    EmitKeyword("protected internal");
                    break;

                case TypeMemberVisibility.FamilyAndAssembly:
                default:
                    EmitKeyword("<Unknown-Visibility>");
                    break;
            }
        }

        public virtual void Emit(string s)
        {
            this._writer.Write(s);
        }

        public virtual void EmitIndent()
        {
            this._writer.Write(new string(' ', this._indentLevel * 2));
        }

        public virtual void EmitNewLine()
        {
            this._writer.WriteLine();
            EmitIndent();
        }

        public virtual void EmitKeyword(string keyword)
        {
            Emit(keyword);
            Emit(" ");
        }

        internal class CodeBlock : IDisposable
        {
            private APIEmitter _apiEmitter;
            private bool _ident;
            public CodeBlock(APIEmitter apiEmitter, bool ident)
            {
                this._apiEmitter = apiEmitter;
                this._ident = ident;

                this._apiEmitter.EmitBlockStart(this._ident);
            }

            public void Dispose()
            {
                if (this._apiEmitter != null)
                {
                    this._apiEmitter.EmitBlockEnd(this._ident);
                    this._apiEmitter = null;
                }
            }
        }


        public object List { get; set; }
    }
}
