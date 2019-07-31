using FilesUpgrade.Model;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Monad
{
    public delegate Out<A> Subsystem<A>();

    public static class Subsystem
    {
        public static Subsystem<A> Return<A>(A value,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            return () => Out<A>.FromValue(value);
        }

        public static Subsystem<Unit> Return(Action f) => () =>
        {
            f();
            return Out<Unit>.FromValue(unit);
        };

        public static Subsystem<A> Fail<A>(Exception exception) => () =>
            Out<A>.FromException(exception);

        public static Subsystem<A> Fail<A>(string message) => () =>
            Out<A>.FromError(message);


        public static Subsystem<B> Bind<A, B>(this Subsystem<A> ma, Func<A, Subsystem<B>> f) => () =>
        {
            try
            {
                // Run ma
                var outA = ma();

                if (outA.IsFailed)
                {
                    // If running ma failed then early out
                    return Out<B>.FromError((Error)outA.Error, outA.Output);
                }
                else
                {
                    // Run the bind function to get the mb monad
                    var mb = f(outA.Value);

                    // Run the mb monad
                    var outB = mb();

                    // Concatenate the output from running ma and mb
                    var output = outA.Output + outB.Output;

                    // Return our result
                    return outB.IsFailed
                        ? Out<B>.FromError((Error)outB.Error, output)
                        : Out<B>.FromValue(outB.Value, output);
                }
            }
            catch (Exception e)
            {
                // Capture exceptions
                return Out<B>.FromException(e);
            }
        };

        public static Subsystem<B> Map<A, B>(
          this Subsystem<A> ma,
          Func<A, B> f) =>
              ma.Bind(a => Return(f(a)));

        public static Subsystem<B> Select<A, B>(
            this Subsystem<A> ma,
            Func<A, B> f) =>
                ma.Bind(a => Return(f(a)));

        public static Subsystem<B> SelectMany<A, B>(
            this Subsystem<A> ma,
            Func<A, Subsystem<B>> f) =>
                ma.Bind(f);

        public static Subsystem<C> SelectMany<A, B, C>(
            this Subsystem<A> ma,
            Func<A, Subsystem<B>> bind,
            Func<A, B, C> project) =>
                ma.Bind(a => bind(a).Map(b => project(a, b)));

        public static Out<A> Execute<A>(this Subsystem<A> ma) =>
            ma();

        public static Subsystem<Unit> Log(string message) => () =>
            Out<Unit>.FromValue(unit, Seq1(message));

        public static Subsystem<Unit> WriteLine(string value) => () =>
            Out<Unit>.FromValue(fun(() => Console.WriteLine(value))());
    }
}
