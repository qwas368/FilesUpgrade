using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Model
{
    public struct Out<A>
    {
        public readonly A Value;
        public readonly Seq<string> Output;
        public readonly Option<Error> Error;

        Out(A value, Seq<string> output, Option<Error> error)
        {
            Value = value;
            Output = output;
            Error = error;
        }

        public static Out<A> FromValue(A value) =>
            new Out<A>(value, Empty, None);

        public static Out<A> FromValue(A value, Seq<string> output) =>
            new Out<A>(value, output, None);

        public static Out<A> FromError(Error error) =>
            new Out<A>(default(A), Empty, error);

        public static Out<A> FromError(Error error, Seq<string> output) =>
            new Out<A>(default(A), output, error);

        public static Out<A> FromError(string message) =>
            new Out<A>(default(A), Empty, Model.Error.FromString(message));

        public static Out<A> FromException(Exception ex) =>
            new Out<A>(default(A), Empty, Model.Error.FromException(ex));

        public bool IsFailed => Error.IsSome;

        public bool IsSucceed => Error.IsNone;
    }
}
