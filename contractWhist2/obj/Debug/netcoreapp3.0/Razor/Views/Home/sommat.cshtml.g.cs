#pragma checksum "C:\Users\richw\source\repos\contractWhist2\contractWhist2\Views\Home\sommat.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "c6e42d9f6e8d406a30a39819747358c651382dd2"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_sommat), @"mvc.1.0.view", @"/Views/Home/sommat.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\richw\source\repos\contractWhist2\contractWhist2\Views\_ViewImports.cshtml"
using contractWhist2;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\richw\source\repos\contractWhist2\contractWhist2\Views\_ViewImports.cshtml"
using contractWhist2.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c6e42d9f6e8d406a30a39819747358c651382dd2", @"/Views/Home/sommat.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"87fd625e483a3bf5af35f16476e6e05d7330cfd8", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_sommat : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 2 "C:\Users\richw\source\repos\contractWhist2\contractWhist2\Views\Home\sommat.cshtml"
  
    ViewData["Title"] = "About";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n<h1>I made this to play with MVC, Javascript and AJAX requests</h1>\r\n\r\n<br />\r\n<p id=\"showMe\">About ");
#nullable restore
#line 9 "C:\Users\richw\source\repos\contractWhist2\contractWhist2\Views\Home\sommat.cshtml"
                Write(ViewBag.Message);

#line default
#line hidden
#nullable disable
            WriteLiteral(" ");
#nullable restore
#line 9 "C:\Users\richw\source\repos\contractWhist2\contractWhist2\Views\Home\sommat.cshtml"
                                 Write(ViewBag.richWaring);

#line default
#line hidden
#nullable disable
            WriteLiteral("</p>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
