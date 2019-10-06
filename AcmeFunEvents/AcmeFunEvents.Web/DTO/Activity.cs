using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AcmeFunEvents.Web.DTO
{
    [Table("AcmeActivity")]
    public class Activity : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Code Required")]
        [Display(Name = "Code")]
        public int Code { get; set; }

        [Required(ErrorMessage = "Name Required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }
        
    }
}