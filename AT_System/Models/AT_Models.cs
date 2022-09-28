using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AT_System.Models
{
    public class _ATForm
    {
        public FI03_AT_Doc AT_Header { get; set; }
        public List<FI03_AT_DocItem> AT_Item { get; set; }
        public List<FI03_AT_FlowApprove> AT_Flow { get; set; }

        public List<vAssetTransfer_Doc> vAT_Doc { get; set; }

        public Guid guid { get; set; }
    }

    public class EmpData_AD
    {
        public string UserAD { get; set; }
        public string EmpID { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
    }

    public class GetAssetList {
        public string ASSET { get; set; }
        public string DESCRIPTION { get; set; }
        public string DESCRIPTION2 { get; set; }
        public string ASSET_CLASS { get; set; }
        public string ASSET_CLASS_NAME { get; set; }
        public string ASSET_CLASS_NAME2 { get; set; }
        public string COST_CENTER { get; set; }
        public string COST_CENTER_NAME { get; set; }
        public string COST_CENTER_NAME2 { get; set; }
        public string LOCATION { get; set; }
        public string LOCATION_NAME { get; set; }
        public string LOCATION_NAME2 { get; set; }
        public string ROOM { get; set; }
        public string CAP_DATE { get; set; }
    }

    public class GetFlowApprove {
        public int StepNo { get; set; }
        public Guid AutoRun { get; set; }
        public Guid FI03_AT_RecCostCenter { get; set; }
        public string EmpID { get; set; }
        public string Email { get; set; }
        public string FirstNameEN { get; set; }
        public string LastNameEN { get; set; }
    }

    public class flowAF {
        public Guid AutoRun { get; set; }
        public string FI03_AT_RecCostCenter { get; set; }
        public string EmpID { get; set; }
        public string Email { get; set; }
    }

    public class _AT_FlwMst
    {
        //public List<FI03_AT_FlowMaster> AT_FlwMst { get; set; }

        public FI03_AT_FlowMaster AT_FlwMst { get; set; }
        public string RecCC_NAME { get; set; }

        //public List<vAssetTransfer_Doc> vAT_Doc { get; set; }
    }

    public class _AT_FlwMst2
    {
        public FI03_AT_RecCostCenter AT_RecCC { get; set; }
        public List<FI03_AT_FlowMaster> AT_FlwMst { get; set; }
    }

    public class _AT_Master
    {
        public FI03_AT_RecCostCenter AT_RecCC { get; set; }

        public List<FI03_AT_FlowMaster> AT_FlwMst { get; set; }
        public List<FI03_AT_CostCenter> AT_CostCenter { get; set; }

        public List<HR01_Employee> AT_Emp { get; set; }

        public List<_AT_FlwEmp> AT_FlwEmp { get; set; }

    }

    public class _AT_FlwEmp
    {
        public FI03_AT_FlowMaster AT_FlwMst { get; set; }
        public HR01_Employee AT_Emp { get; set; }
    }
}
