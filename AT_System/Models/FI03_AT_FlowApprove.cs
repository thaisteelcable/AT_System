//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AT_System.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FI03_AT_FlowApprove
    {
        public string Client { get; set; }
        public System.Guid AutoRun { get; set; }
        public Nullable<System.Guid> FI03_AT_Doc { get; set; }
        public Nullable<System.Guid> FI03_AT_FlowMaster { get; set; }
        public string FlowState { get; set; }
        public string FlowStatus { get; set; }
        public string EmpID { get; set; }
        public string EmpFirstName { get; set; }
        public string EmpLastName { get; set; }
        public string EmpEmail { get; set; }
        public string ApproveStatus { get; set; }
        public string DateApprove { get; set; }
        public string TimeApprove { get; set; }
        public string UserCreate { get; set; }
        public string DateCreate { get; set; }
        public string Remark { get; set; }
        public string Flag { get; set; }
        public string FI03_AT_RecCostCenter { get; set; }
    }
}
