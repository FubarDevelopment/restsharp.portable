using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Fubar Development Junker, RestSharp.Portable community and others")]
[assembly: AssemblyCopyright("Copyright © RestSharp.Portable project 2013-2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

[assembly: AssemblyVersion("4.0")]
[assembly: AssemblyFileVersion("4.0.0")]
[assembly: AssemblyInformationalVersion("4.0.0")]

[assembly: InternalsVisibleTo("FubarCoder.RestSharp.Portable.OAuth1, PublicKey=" +
"00240000048000009400000006020000002400005253413100040000010001000f8415df6f1232" +
"a8e57ec65d511e606ce7ecdd04e8dfeff5518e27ad449b9ccce658c1b0dc5e73e86626540b7a1e" +
"c3c2f9e69f37db4626ad2207a4b4c5c3bf064d2a7c7766961b5f55d16d4082753d0c4be118e658" +
"9bb2ee7a911ad43721beea1f034c3d8a87c97f5de3e575cc0db2db79d396e69eeef2461ff8615f" +
"48df7bba")]

[assembly: InternalsVisibleTo("FubarCoder.RestSharp.Portable.OAuth1.Tests, PublicKey=" +
"00240000048000009400000006020000002400005253413100040000010001000f8415df6f1232" +
"a8e57ec65d511e606ce7ecdd04e8dfeff5518e27ad449b9ccce658c1b0dc5e73e86626540b7a1e" +
"c3c2f9e69f37db4626ad2207a4b4c5c3bf064d2a7c7766961b5f55d16d4082753d0c4be118e658" +
"9bb2ee7a911ad43721beea1f034c3d8a87c97f5de3e575cc0db2db79d396e69eeef2461ff8615f" +
"48df7bba")]

[assembly: InternalsVisibleTo("FubarCoder.RestSharp.Portable.Tests, PublicKey=" +
"00240000048000009400000006020000002400005253413100040000010001000f8415df6f1232" +
"a8e57ec65d511e606ce7ecdd04e8dfeff5518e27ad449b9ccce658c1b0dc5e73e86626540b7a1e" +
"c3c2f9e69f37db4626ad2207a4b4c5c3bf064d2a7c7766961b5f55d16d4082753d0c4be118e658" +
"9bb2ee7a911ad43721beea1f034c3d8a87c97f5de3e575cc0db2db79d396e69eeef2461ff8615f" +
"48df7bba")]

#if !NOT_CLS_COMPLIANT
[assembly: CLSCompliant(true)]
#endif
