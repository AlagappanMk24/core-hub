﻿namespace Core_API.Application.Contracts.DTOs.Request
{
    public class RoleMenuPermissionDto
    {
        public int? Id { get; set; }
        public string RoleId { get; set; }
        public string MenuName { get; set; }
        public int PermissionId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
