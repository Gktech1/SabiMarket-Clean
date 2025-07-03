using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Enum
{
    public enum BuildingTypeEnum
    {
        [Display(Name = "Open Space")]
        OpenSpace = 1,

        [Display(Name = "Kiosk")]
        Kiosk = 2,

        [Display(Name = "Shop")]
        Shop = 3,

        [Display(Name = "Warehouse")]
        Warehouse = 4
    }

    /* public enum BuildingTypeEnum
     {
         OpenSpace = 1,
         Kiosk = 2,
         Shop = 3,
         Warehouse = 4
     }*/
}
