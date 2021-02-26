using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fp.Sg
{
    [Generator]
    public class StructureGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver recv) return;
            foreach (var cds in recv.Decls)
            {
                var sem = context.Compilation.GetSemanticModel(cds.SyntaxTree);
                var nts = sem.GetDeclaredSymbol(cds) as INamedTypeSymbol;
                if (nts == null) continue;
                if (IsDerived(nts, "Fp.Structures.Structure", out _))
                    MakeSource(in context, nts);
            }
        }

        private void MakeSource(in GeneratorExecutionContext context, INamedTypeSymbol nts)
        {
            string name = nts.Name;
            string? ns = !string.IsNullOrWhiteSpace(nts.ContainingNamespace?.Name)
                ? GetFullyQualifiedName(nts.ContainingNamespace!)
                : null;
            var sbInitBuilder = new StringBuilder();
            var sbFieldsBuilder = new StringBuilder();
            var sbFieldsMeta = new StringBuilder();
            foreach (var member in nts.GetMembers())
            {
                if (member is not IFieldSymbol {IsStatic: true} fs) continue;
                var genFieldInfo = GetFieldType((fs.Type as INamedTypeSymbol)!);
                if (genFieldInfo == null) continue;
                string fieldName = fs.Name;
                sbInitBuilder.Append(@$"
                ({name}.{fieldName}, ");
                sbInitBuilder.Append(genFieldInfo.CanRead
                    ? $"(i, e, ctx) => i.{fieldName} = e.Read<{genFieldInfo.Type}>(ctx), "
                    : "null, ");
                sbInitBuilder.Append(genFieldInfo.CanWrite
                    ? $"(i, e, ctx) => e.Write<{genFieldInfo.Type}>(ctx, i.{fieldName})"
                    : "null");
                sbInitBuilder.Append("),");
                if (genFieldInfo.CanRead || genFieldInfo.CanWrite)
                {
                    sbFieldsBuilder.Append($@"
        public {genFieldInfo.Type} {fieldName};");
                    sbFieldsMeta.Append(@$"
                {{{name}.{fieldName}, new RefExpression<{genFieldInfo.Type}>(() => {fieldName})}},");
                }
            }

            context.AddSource($"{name}Instance.cs", @$"
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fp.Structures;

{(ns != null ? @$"namespace {ns}
{{" : "")}
    public class {name}Instance : StructureInstance
    {{{sbFieldsBuilder}
        private static List<(Element, Action<{name}Instance, Expression, StructureContext>, Action<{name}Instance, WritableExpression, StructureContext>)> _base;
        private static List<(Element, Action<{name}Instance, Expression, StructureContext>, Action<{name}Instance, WritableExpression, StructureContext>)> _rev;

        private readonly Dictionary<Element, Expression> _exprMap;
        private readonly List<Expression> _baseInst;
        private readonly List<WritableExpression> _revInst;
        public {name}Instance()
        {{
            _exprMap = new Dictionary<Element, Expression> {{{sbFieldsMeta}
            }};
            _baseInst = new();
            _revInst = new();
            for (int i = 0; i < _base!.Count; i++)
            {{
                var x = _base[i];
                _baseInst.Add(x.Item2 != null ? ((Expression)x.Item1).GetMetaExpression(_exprMap) : null);
            }}
            for (int i = 0; i < _rev!.Count; i++)
            {{
                var x = _rev[i];
                _revInst.Add(x.Item3 != null ? (WritableExpression)((Expression)x.Item1).GetMetaExpression(_exprMap) : null);
            }}
        }}

        static {name}Instance()
        {{
            _base = BuildLayout<{name}Instance>(new (Element, Action<{name}Instance, Expression, StructureContext>, Action<{name}Instance, WritableExpression, StructureContext>)[]
            {{{sbInitBuilder}
            }});
            _rev = new(_base);
            _rev.Reverse();
        }}

        public override void Read(StructureContext context)
        {{
            for(int i = 0; i < _base!.Count; i++)
                _base[i].Item2?.Invoke(this, _baseInst[i]!, context);
        }}

        public override void Write(StructureContext context)
        {{
            for(int i = 0; i < _rev!.Count; i++)
                _rev[i].Item3?.Invoke(this, _revInst[i]!, context);
        }}

        public static {name}Instance Read(Stream stream) => Structure.Read<{name}Instance>(stream);
    }}
{(ns != null ? @"
}" : "")}
");
        }

        private static GenFieldInfo? GetFieldType(INamedTypeSymbol elementType)
        {
            string? gp;
            if (IsDerived(elementType, "Fp.Structures.Expression<>", out gp))
                return new GenFieldInfo(true, false, gp);
            if (IsDerived(elementType, "Fp.Structures.WritableExpression<>", out gp))
                return new GenFieldInfo(true, true, gp);
            return null;
        }

        private record GenFieldInfo(bool CanRead, bool CanWrite, string? Type = null);

        private static string GetFullyQualifiedName(INamespaceOrTypeSymbol ts)
        {
            var ns = ts.ContainingNamespace;
            return string.IsNullOrWhiteSpace(ns?.Name) ? ts.Name : $"{GetFullyQualifiedName(ns!)}.{ts.Name}";
        }

        private static bool IsDerived(INamedTypeSymbol ts, string typeName, out string? genericParam)
        {
            genericParam = null;
            while (true)
            {
                if (ts.IsGenericType)
                {
                    var ts2 = ts.ConstructUnboundGenericType();
                    if (ts2.ToString() == typeName)
                    {
                        genericParam = ts.TypeArguments[0].ToString();
                        return true;
                    }
                }
                else
                {
                    if (ts.ToString() == typeName) return true;
                }

                if (ts.BaseType == null) return false;
                ts = ts.BaseType;
            }
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<TypeDeclarationSyntax> Decls { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is TypeDeclarationSyntax cds) Decls.Add(cds);
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
