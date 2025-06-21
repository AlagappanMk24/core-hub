import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RoleManagementService } from '../../../services/role-management.service';
import { Role } from '../../../interfaces/auth/role/role.interface';
import { Menu } from '../../../interfaces/auth/role/menu.interface';
import { Permission } from '../../../interfaces/auth/role/permission.interface';
import { RoleMenuPermission } from '../../../interfaces/auth/role/role-menu-permission.interface';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-user-role-management',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './user-role-management.component.html',
  styleUrls: ['./user-role-management.component.css'],
  animations: [
    trigger('toast', [
      transition(':enter', [
        style({ transform: 'translateY(20px)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateY(20px)', opacity: 0 })),
      ]),
    ]),
  ],
})
export class UserRoleManagementComponent implements OnInit {
  roleForm: FormGroup;
  roles: Role[] = [];
  menus: Menu[] = [];
  permissions: Permission[] = [];
  roleMenuPermissions: RoleMenuPermission[] = [];
  permissionsChanged = false;
  isCollapsed: boolean[] = [];
  saving = false;
  showSuccessToast = false;
  showErrorToast = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private roleManagementService: RoleManagementService
  ) {
    this.roleForm = this.fb.group({
      roleId: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadMenus();
    this.loadPermissions();
  }

  loadRoles(): void {
    this.roleManagementService.getRoles().subscribe({
      next: (roles) => (this.roles = roles),
      error: (error) => console.error('Error loading roles:', error),
    });
  }

  loadMenus(): void {
    this.roleManagementService.getMenus().subscribe({
      next: (menus) => {
        this.menus = menus;
        this.isCollapsed = new Array(menus.length).fill(true);
      },
      error: (error) => console.error('Error loading menus:', error),
    });
  }

  loadPermissions(): void {
    this.roleManagementService.getPermissions().subscribe({
      next: (permissions) => (this.permissions = permissions),
      error: (error) => console.error('Error loading permissions:', error),
    });
  }

  onRoleChange(event: Event): void {
    const roleId = (event.target as HTMLSelectElement).value;
    if (roleId) {
      this.roleManagementService.getRoleMenuPermissions(roleId).subscribe({
        next: (permissions) => {
          this.roleMenuPermissions = permissions;
          this.permissionsChanged = false;
          this.isCollapsed = new Array(this.menus.length).fill(true);
        },
        error: (error) => {
          console.error('Error loading permissions:', error);
          this.showError('Failed to load permissions');
        },
      });
    } else {
      this.roleMenuPermissions = [];
    }
  }

  isMenuEnabled(menuId: string): boolean {
    return this.roleMenuPermissions.some(
      (p) => p.menuName === menuId && p.isEnabled
    );
  }

  isPermissionChecked(menuId: string, permissionId: number): boolean {
    return this.roleMenuPermissions.some(
      (p) =>
        p.menuName === menuId &&
        p.permissionId === permissionId &&
        p.isEnabled
    );
  }

  toggleMenu(menuId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.roleMenuPermissions
      .filter((p) => p.menuName === menuId)
      .forEach((p) => (p.isEnabled = checked));
    this.permissionsChanged = true;
  }

  togglePermission(menuId: string, permissionId: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const existing = this.roleMenuPermissions.find(
      (p) => p.menuName === menuId && p.permissionId === permissionId
    );
    if (existing) {
      existing.isEnabled = checked;
    } else {
      this.roleMenuPermissions.push({
        roleId: this.roleForm.get('roleId')?.value,
        menuName: menuId,
        permissionId,
        isEnabled: checked,
      });
    }
    this.permissionsChanged = true;
  }

  toggleCollapse(index: number): void {
    this.isCollapsed[index] = !this.isCollapsed[index];
  }

  onSave(): void {
    if (this.roleForm.valid && this.permissionsChanged) {
      this.saving = true;
      this.roleManagementService.saveRoleMenuPermissions(this.roleMenuPermissions).subscribe({
        next: () => {
          this.saving = false;
          this.permissionsChanged = false;
          this.showSuccess();
        },
        error: (error) => {
          this.saving = false;
          console.error('Error saving permissions:', error);
          this.showError('Failed to save permissions');
        },
      });
    }
  }

  showSuccess(): void {
    this.showSuccessToast = true;
    setTimeout(() => (this.showSuccessToast = false), 3000);
  }

  showError(message: string): void {
    this.errorMessage = message;
    this.showErrorToast = true;
    setTimeout(() => (this.showErrorToast = false), 4000);
  }
}