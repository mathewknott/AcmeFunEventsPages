using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace AcmeFunEvents.Web.DTO
{
    [Table("AcmeRegistration")]
    public class Registration : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Comments Required")]
        [Display(Name = "Comments")]
        public string Comments { get; set; }

        public virtual User User { get; set; }
        
        public virtual Activity Activity { get; set; }

        [JsonIgnore]
        [Display(Name = "User")]
        public Guid? UserId { get; set; }

        [JsonIgnore]
        [Display(Name = "Activity")]
        public Guid? ActivityId { get; set; }
    }
}