using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Hal;
using Service.WebApi.Controllers.Web;

namespace Service.WebApi.Resources
{
#pragma warning disable CS4014
    public class SchoolResourceMap : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            Map<SchoolResource>()
                .LinkMeta<RestController>(meta =>
                {
                    // meta.Url(RelationTypes.Self, (c, r) => c.GetSchool(r.Id));
                    // meta.Url("students", (c, r) => c.GetStudents(r.Id));
                });

            Map<StudentResource>()
                .LinkMeta<RestController>(meta =>
                {
                    meta.Url("update", (c, r) => c.UpdateStudent(r.Id, default));
                    meta.UrlTemplate<Guid, IActionResult>("address", c => c.GetStudentAddress);
                })

                .LinkMeta(meta =>
                {
                    meta.Href("student-info", HttpMethod.Get, "https://students.org/info/{studentId}");
                });
        }
    }
}