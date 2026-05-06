import { Component, OnInit } from '@angular/core';

interface Permission {
  id: string;
  name: string;
  create: boolean;
  read: boolean;
  write: boolean;
  delete: boolean;
}

interface FieldPermission {
  read: boolean;
  write: boolean;
}

interface Role {
  id: string;
  name: string;
  displayName: string;
  description: string;
  permissions: Permission;
  fieldPermissions: FieldPermission;
}

@Component({
  selector: 'app-role-permission',
  templateUrl: './role-management.component.html',
  styleUrls: ['./role-management.component.css']
})
export class RoleMangementComponent implements OnInit {
  
  searchTerm: string = '';
  selectedPermissionType: string = 'artifact';
  
  roles: Role[] = [
    {
      id: 'everyone',
      name: 'Everyone',
      displayName: 'Everyone',
      description: 'Default permissions for all users',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: false,
        read: true,
        write: false,
        delete: false
      },
      fieldPermissions: {
        read: true,
        write: false
      }
    },
    {
      id: 'business_reviewer',
      name: 'Business reviewer',
      displayName: 'Business reviewer',
      description: 'ro.business_reviewer',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: false,
        read: true,
        write: false,
        delete: false
      },
      fieldPermissions: {
        read: true,
        write: false
      }
    },
    {
      id: 'contributor',
      name: 'Contributor',
      displayName: 'Contributor',
      description: 'ro.contributor',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: false,
        read: true,
        write: false,
        delete: false
      },
      fieldPermissions: {
        read: true,
        write: false
      }
    },
    {
      id: 'final_approver',
      name: 'Final approver',
      displayName: 'Final approver',
      description: 'ro.final_approver',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: true,
        read: true,
        write: true,
        delete: true
      },
      fieldPermissions: {
        read: true,
        write: true
      }
    },
    {
      id: 'it_operations_reviewer',
      name: 'IT & Operations reviewer',
      displayName: 'IT & Operations reviewer',
      description: 'ro.it_operations_reviewer',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: false,
        read: true,
        write: false,
        delete: false
      },
      fieldPermissions: {
        read: true,
        write: false
      }
    },
    {
      id: 'project_manager',
      name: 'Project manager',
      displayName: 'Project manager',
      description: 'ro.project_manager',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: true,
        read: true,
        write: true,
        delete: true
      },
      fieldPermissions: {
        read: true,
        write: true
      }
    },
    {
      id: 'reader',
      name: 'Reader',
      displayName: 'Reader',
      description: 'ro.reader',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: false,
        read: true,
        write: false,
        delete: false
      },
      fieldPermissions: {
        read: true,
        write: false
      }
    },
    {
      id: 'risk_compliance_reviewer',
      name: 'Risk & Compliance reviewer',
      displayName: 'Risk & Compliance reviewer',
      description: 'ro.risk_compliance_reviewer',
      permissions: {
        id: 'artifact_permissions',
        name: 'Artifact, blueprint and blueprint version permissions',
        create: false,
        read: true,
        write: false,
        delete: false
      },
      fieldPermissions: {
        read: true,
        write: false
      }
    }
  ];

  filteredRoles: Role[] = [];

  ngOnInit() {
    this.filteredRoles = [...this.roles];
  }

  filterRoles() {
    if (!this.searchTerm.trim()) {
      this.filteredRoles = [...this.roles];
      return;
    }

    this.filteredRoles = this.roles.filter(role => 
      role.displayName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      role.description.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  clearFilter() {
    this.searchTerm = '';
    this.filteredRoles = [...this.roles];
  }

  onPermissionChange(roleId: string, permissionType: string, value: boolean) {
    const role = this.roles.find(r => r.id === roleId);
    if (role) {
      switch(permissionType) {
        case 'create':
          role.permissions.create = value;
          break;
        case 'read':
          role.permissions.read = value;
          break;
        case 'write':
          role.permissions.write = value;
          break;
        case 'delete':
          role.permissions.delete = value;
          break;
        case 'fieldRead':
          role.fieldPermissions.read = value;
          break;
        case 'fieldWrite':
          role.fieldPermissions.write = value;
          break;
      }
    }
  }

  selectAllPermissions(permissionType: string) {
    this.roles.forEach(role => {
      switch(permissionType) {
        case 'create':
          role.permissions.create = true;
          break;
        case 'read':
          role.permissions.read = true;
          break;
        case 'write':
          role.permissions.write = true;
          break;
        case 'delete':
          role.permissions.delete = true;
          break;
        case 'fieldRead':
          role.fieldPermissions.read = true;
          break;
        case 'fieldWrite':
          role.fieldPermissions.write = true;
          break;
      }
    });
  }

  deselectAllPermissions(permissionType: string) {
    this.roles.forEach(role => {
      switch(permissionType) {
        case 'create':
          role.permissions.create = false;
          break;
        case 'read':
          role.permissions.read = false;
          break;
        case 'write':
          role.permissions.write = false;
          break;
        case 'delete':
          role.permissions.delete = false;
          break;
        case 'fieldRead':
          role.fieldPermissions.read = false;
          break;
        case 'fieldWrite':
          role.fieldPermissions.write = false;
          break;
      }
    });
  }

  savePermissions() {
    // Implement save logic here
    // You can add your API call here
  }

  cancelChanges() {
    // Implement cancel logic here
    // You can reload original data or navigate away
  }

  getRoleIcon(roleId: string): string {
    switch(roleId) {
      case 'everyone': return 'people';
      case 'business_reviewer': return 'business';
      case 'contributor': return 'person_add';
      case 'final_approver': return 'verified';
      case 'it_operations_reviewer': return 'settings';
      case 'project_manager': return 'manage_accounts';
      case 'reader': return 'visibility';
      case 'risk_compliance_reviewer': return 'security';
      default: return 'person';
    }
  }

  trackByRole(index: number, role: Role): string {
    return role.id;
  }
}