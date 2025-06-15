import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string;
  gender: 'Male' | 'Female';
  avatar: string;
  address?: string;
}

@Component({
  selector: 'app-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.css'],
  imports: [CommonModule],
})
export class CustomerComponent implements OnInit {
  customers: Customer[] = [
    {
      id: 1,
      name: 'John Deo',
      email: 'johndeo213@gmail.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/john-deo.jpg',
      address: '2738 Hog Camp Road',
    },
    {
      id: 2,
      name: 'Shelly Goods',
      email: 'shellygoods481@gmail.com',
      phone: '+3375700647',
      gender: 'Female',
      avatar: 'assets/avatars/shelly-goods.jpg',
    },
    {
      id: 3,
      name: 'Robert Bruno',
      email: 'robertbruno419@gmail.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/robert-bruno.jpg',
    },
    {
      id: 4,
      name: 'John Cards',
      email: 'john.cards349@gmail.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/john-cards.jpg',
    },
    {
      id: 5,
      name: 'Adriana Watson',
      email: 'adrianawatson878@gmail.com',
      phone: '+8375740647',
      gender: 'Female',
      avatar: 'assets/avatars/adriana-watson.jpg',
    },
    {
      id: 6,
      name: 'John Deo',
      email: 'johndeo482398@gmail.com',
      phone: '+8347570046',
      gender: 'Male',
      avatar: 'assets/avatars/john-deo-2.jpg',
    },
    {
      id: 7,
      name: 'Mark Ruffalo',
      email: 'markruffalo375@gmail.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/mark-ruffalo.jpg',
    },
    {
      id: 8,
      name: 'Bethany Jackson',
      email: 'bethanyjackson75@gmail.com',
      phone: '+3375700647',
      gender: 'Female',
      avatar: 'assets/avatars/bethany-jackson.jpg',
    },
    {
      id: 9,
      name: 'Christine Huston',
      email: 'christinehuston46@gmail.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/christine-huston.jpg',
    },
    {
      id: 10,
      name: 'Anne Jacob',
      email: 'annejacob40@email.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/anne-jacob.jpg',
    },
    {
      id: 11,
      name: 'James Mulican',
      email: 'jamesmulican5346@gmail.com',
      phone: '+3375700647',
      gender: 'Male',
      avatar: 'assets/avatars/james-mulican.jpg',
    },
  ];

  selectedCustomer: Customer | null = null;
  isDrawerOpen = false;

  ngOnInit(): void {}

  selectCustomer(customer: Customer): void {
    this.selectedCustomer = customer;
    this.isDrawerOpen = true;
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.selectedCustomer = null;
  }

  addCustomer(): void {
    // Implement add customer logic
    console.log('Add customer clicked');
  }

  getAvatarUrl(customer: Customer): string {
    // Generate avatar based on name initials or use a default avatar service
    const initials = customer.name
      .split(' ')
      .map((n) => n[0])
      .join('');
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(
      customer.name
    )}&background=random&color=fff&size=40`;
  }

  getGenderBadgeClass(gender: string): string {
    return gender === 'Male' ? 'badge-male' : 'badge-female';
  }
}
