using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace conexionBBDD
{
    [Table("users")]
    public class users
    {
        [Key]
        public int idusers { get; set; }

        public string name { get; set; }
    }
}
