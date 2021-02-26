using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591
namespace Fp.Structures
{
    public record str(Expression Offset, Expression Length, Encoding Encoding) : WritableExpression<string>
    {
        public override IEnumerable<Element> Dependencies => new[] {Offset, Length};

        public override Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) => this with
        {
            Offset = Offset.GetSelfMetaExpression(mapping), Length = Length.GetSelfMetaExpression(mapping)
        };

        public override T? Read<T>(StructureContext context) where T : default
        {
            context.Seek(Offset.ReadUnmanaged<long>(context));
            byte[] buffer = new byte[Length.ReadUnmanaged<int>(context)];
            context.Stream.ReadArray(buffer, 0, buffer.Length);
            return GetValueOrDefault<string, T>(Encoding.GetString(buffer));
        }

        public override void Write<T>(StructureContext context, T value)
        {
            context.Seek(Offset.ReadUnmanaged<long>(context));
            string str = GetValueOrDefault<T, string>(value) ?? throw new NullReferenceException();
            byte[] buffer = Encoding.GetBytes(str);
            context.Stream.Write(buffer, 0, Math.Min(buffer.Length, Length.ReadUnmanaged<int>(context)));
        }
    }

    public record ntstr8(Expression Offset) : WritableExpression<string>
    {
        public override IEnumerable<Element> Dependencies => new[] {Offset};

        public override Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) => this with
        {
            Offset = Offset.GetSelfMetaExpression(mapping)
        };

        public override T? Read<T>(StructureContext context) where T : default
        {
            context.Seek(Offset.ReadUnmanaged<long>(context));
            return GetValueOrDefault<string, T>(Processor.Instance.ReadUtf8String(context.Stream, out _));
        }

        public override void Write<T>(StructureContext context, T value)
        {
            context.Seek(Offset.ReadUnmanaged<long>(context));
            string str = GetValueOrDefault<T, string>(value) ?? throw new NullReferenceException();
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            context.Stream.Write(buffer, 0, buffer.Length);
        }
    }

    public record ntstr16(Expression Offset) : WritableExpression<string>
    {
        public override IEnumerable<Element> Dependencies => new[] {Offset};

        public override Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) => this with
        {
            Offset = Offset.GetSelfMetaExpression(mapping)
        };

        public override T? Read<T>(StructureContext context) where T : default
        {
            context.Seek(Offset.ReadUnmanaged<long>(context));
            return GetValueOrDefault<string, T>(Processor.Instance.ReadUtf16String(context.Stream, out _));
        }

        public override void Write<T>(StructureContext context, T value)
        {
            context.Seek(Offset.ReadUnmanaged<long>(context));
            string str = GetValueOrDefault<T, string>(value) ?? throw new NullReferenceException();
            byte[] buffer = Encoding.Unicode.GetBytes(str);
            context.Stream.Write(buffer, 0, buffer.Length);
        }
    }

    public record vstr : ValueExpression<string>
    {
        public vstr(string? Value) : base(Value)
        {
        }

        public static implicit operator vstr(string value) => new(value);
        public override T? Read<T>(StructureContext context) where T : default => GetValueOrDefault<string, T>(Value);
    }

    public partial record Expression
    {
        public static implicit operator Expression(string value) => new vstr(value);
    }

    public partial class Structure
    {
        public static str str(Expression offset, Expression length, Encoding encoding) => new(offset, length, encoding);
        public static str str8(Expression offset, Expression length) => new(offset, length, Encoding.UTF8);
        public static str str16(Expression offset, Expression length) => new(offset, length, Encoding.Unicode);
        public static ntstr8 ntstr8(Expression offset) => new(offset);
        public static ntstr16 ntstr16(Expression offset) => new(offset);
        public static vstr vstr(string value) => new(value);
    }
}
