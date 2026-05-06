// home.component.ts
import {
  Component, OnInit, OnDestroy, HostListener,
  ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit, OnDestroy {

  /* ─── Theme ─── */
  theme: 'dark' | 'light' = 'dark';

  /* ─── UI state ─── */
  menuOpen  = false;
  scrolled  = false;
  heroReady = false;

  /* ─── Particles ─── */
  particles: Record<string, string>[] = [];

  /* ─── Avatar colors for social proof ─── */
  avColors = ['#a855f7','#3b82f6','#10b981','#ec4899','#f59e0b'];

  /* ─── Invoice items in hero card ─── */
  invoiceItems = [
    { name: 'Brand Identity Design', price: '$3,200', color: '#a855f7' },
    { name: 'Web Development',       price: '$4,000', color: '#ec4899' },
    { name: 'Strategy Consulting',   price: '$1,200', color: '#06b6d4' },
  ];

  /* ─── Marquee ─── */
  marquee = [
    'Smart Invoice Builder',
    'Real-Time Payment Tracking',
    'Automated Reminders',
    'Revenue Analytics',
    'B2B & B2C Support',
    'Bank-Grade Security',
    'Recurring Invoices',
    'Multi-Currency',
    'Custom Branding',
    'Role-Based Access',
  ];

  /* ─── Stats ─── */
  stats = [
    { val: '$2.4B+', lbl: 'Total Invoiced',         pct: 88 },
    { val: '12K+',   lbl: 'Businesses Worldwide',   pct: 72 },
    { val: '99.9%',  lbl: 'Platform Uptime',        pct: 99 },
    { val: '< 3s',   lbl: 'Invoice Creation Speed', pct: 95 },
  ];

  /* ─── How it works ─── */
  howSteps = [
    {
      num: '01',
      title: 'Create Your Account',
      desc:  'Sign up free in 30 seconds. Add your logo, business details and tax settings once — they populate every invoice automatically.',
      icon:  `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>`
    },
    {
      num: '02',
      title: 'Build Your Invoice',
      desc:  'Add your client, drag in line items, set due date and discounts. Live preview updates as you type. Ready in under 60 seconds.',
      icon:  `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>`
    },
    {
      num: '03',
      title: 'Send & Get Paid',
      desc:  'Email in one click. Track opens and payments live. Automated reminders fire if payment is overdue. Your cash flow, on autopilot.',
      icon:  `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.15 12 19.79 19.79 0 0 1 1.08 3.42 2 2 0 0 1 3.07 1h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L7.09 8.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 21 16z"/></svg>`
    },
  ];

  /* ─── Pricing ─── */
  pricing = [
    {
      name: 'Starter', price: '0', cta: 'Start Free',
      desc: 'Perfect for freelancers getting started.',
      badge: null, featured: false,
      feats: ['5 invoices/month','Basic templates','Email delivery','PDF export','1 user']
    },
    {
      name: 'Pro', price: '29', cta: 'Get Pro',
      desc: 'For growing businesses that need more.',
      badge: 'MOST POPULAR', featured: true,
      feats: ['Unlimited invoices','Custom branding','Auto reminders','Analytics dashboard','5 users','Priority support']
    },
    {
      name: 'Enterprise', price: '99', cta: 'Contact Sales',
      desc: 'For teams that need full control and scale.',
      badge: null, featured: false,
      feats: ['Everything in Pro','Unlimited users','API access','SSO / SAML','Dedicated account manager','SLA guarantee']
    },
  ];

  constructor(
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Load saved theme preference
    const saved = localStorage.getItem('ci-theme') as 'dark' | 'light' | null;
    if (saved) {
      this.theme = saved;
    } else {
      // Respect OS preference
      this.theme = window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }

    // Generate particles
    this.particles = Array.from({ length: 22 }, () => this.makeParticle());

    // Trigger hero entrance
    setTimeout(() => {
      this.heroReady = true;
      this.cdr.markForCheck();
    }, 120);
  }

  ngOnDestroy(): void {}

  /* ─── Theme toggle ─── */
  toggleTheme(): void {
    this.theme = this.theme === 'dark' ? 'light' : 'dark';
    localStorage.setItem('ci-theme', this.theme);
    this.cdr.markForCheck();
  }

  /* ─── Navigation ─── */
  onGetStarted(): void { this.router.navigate(['/auth/register']); }
  onLogin():      void { this.router.navigate(['/auth/login']); }
  onDemo():       void { document.getElementById('features')?.scrollIntoView({ behavior: 'smooth' }); }

  /* ─── Mobile menu ─── */
  toggleMenu(): void { this.menuOpen = !this.menuOpen; }
  closeMenu():  void { this.menuOpen = false; }

  /* ─── Scroll listener ─── */
  @HostListener('window:scroll')
  onScroll(): void {
    const y = window.scrollY;
    const wasScrolled = this.scrolled;
    this.scrolled = y > 40;
    if (wasScrolled !== this.scrolled) this.cdr.markForCheck();
  }

  /* ─── Particle generator ─── */
  private makeParticle(): Record<string, string> {
    const size    = Math.random() * 3 + 1.5;
    const left    = Math.random() * 100;
    const delay   = Math.random() * 14;
    const dur     = Math.random() * 10 + 12;
    const colors  = ['rgba(168,85,247,0.5)', 'rgba(236,72,153,0.4)', 'rgba(99,102,241,0.4)', 'rgba(139,92,246,0.5)'];
    const color   = colors[Math.floor(Math.random() * colors.length)];
    return {
      width:                `${size}px`,
      height:               `${size}px`,
      left:                 `${left}%`,
      background:           color,
      animationDuration:    `${dur}s`,
      animationDelay:       `${delay}s`,
    };
  }
}