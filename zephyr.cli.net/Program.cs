using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace zephyr.cli.net
{
    class Program
    {
        //todo:
        //  - add array bounds checking on args
        //  - make sure target zephyr lib exists
        //  - try/catch the assembly load write nice errors
        //  - same for invoking main
        //  - plus add 'help'
        static void Main(string[] args)
        {
            string zephyrLib = $"zephyr.{args[0]}";
            AssemblyName an = new AssemblyName( zephyrLib );
            Assembly z = Assembly.Load( an );

            Type t = z.GetType( "ZephyrCommandLine", throwOnError: true, ignoreCase: true );
            object zephyrCli = Activator.CreateInstance( t );

            MethodInfo main = zephyrCli.GetType().GetRuntimeMethod( "Main", new Type[] { typeof( string[] ) } );

            string[] a = new string[args.Length - 1];
            for( int i = 1; i < args.Length; i++ )
                a[i - 1] = args[i];
            main.Invoke( zephyrCli, new object[] { a } );
        }
    }
}