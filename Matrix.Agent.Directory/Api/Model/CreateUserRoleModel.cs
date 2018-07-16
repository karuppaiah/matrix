﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Matrix.Agent.Directory.Api.Model
{
    public class CreateUserRoleModel
    {
        [Required]
        public Guid Application { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}