export interface Permission {
  id: number;
  entityName: string; // e.g., Orders
  action: string; // e.g., View
}