export interface RoleMenuPermission {
  id?: number;
  roleId: string; // AspNetRoles Id
  menuName: string; // e.g., Orders
  permissionId: number;
  isEnabled: boolean;
}
