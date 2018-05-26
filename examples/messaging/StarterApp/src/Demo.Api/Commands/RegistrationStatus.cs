using System;

namespace Demo.Api.Commands
{
   public class RegistrationStatus
   {
       public string ReferenceNumber { get; set; }
       public bool IsSuccess { get; set; }
       public DateTime DateAccountActive { get; set; }
   } 
}