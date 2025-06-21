// home.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  
  private animationId: number = 0;
  private isAnimating: boolean = false;

  constructor(private router: Router) { }

  ngOnInit(): void {
    this.startAnimations();
  }

  ngOnDestroy(): void {
    this.stopAnimations();
  }

  /**
   * Handle the Get Started button click
   */
  onGetStarted(): void {
    window.location.href='auth/login';
    // Add your navigation logic here
    // Example: this.router.navigate(['/products']);
    
    // Add button click animation
    this.animateButton();
  }

  /**
   * Start subtle animations for the illustration elements
   */
  private startAnimations(): void {
    this.isAnimating = true;
    this.animate();
  }

  /**
   * Stop all animations
   */
  private stopAnimations(): void {
    this.isAnimating = false;
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
    }
  }

  /**
   * Main animation loop for floating elements
   */
  private animate(): void {
    if (!this.isAnimating) return;

    const time = Date.now() * 0.001;
    
    // Animate floating elements
    this.animateFloatingElements(time);
    
    // Continue animation loop
    this.animationId = requestAnimationFrame(() => this.animate());
  }

  /**
   * Animate floating elements with subtle movements
   */
  private animateFloatingElements(time: number): void {
    const elements = [
      { selector: '.sales-badge', amplitude: 2, speed: 1.5 },
      { selector: '.discount-badge', amplitude: 3, speed: 2 },
      { selector: '.delivery-truck', amplitude: 1, speed: 1 },
      { selector: '.gift-box', amplitude: 2, speed: 1.8 },
      { selector: '.package-1', amplitude: 1.5, speed: 1.2 },
      { selector: '.package-2', amplitude: 2.5, speed: 0.8 }
    ];

    elements.forEach(element => {
      const el = document.querySelector(element.selector) as HTMLElement;
      if (el) {
        const yOffset = Math.sin(time * element.speed) * element.amplitude;
        el.style.transform = `${el.style.transform.replace(/translateY\([^)]*\)/, '')} translateY(${yOffset}px)`;
      }
    });
  }

  /**
   * Animate the get started button on click
   */
  private animateButton(): void {
    const button = document.querySelector('.get-started-btn') as HTMLElement;
    if (button) {
      button.style.transform = 'scale(0.95)';
      setTimeout(() => {
        button.style.transform = 'scale(1)';
      }, 150);
    }
  }

  /**
   * Handle window resize events
   */
  onWindowResize(): void {
    // Handle responsive adjustments if needed
    this.adjustLayoutForScreenSize();
  }

  /**
   * Adjust layout based on screen size
   */
  private adjustLayoutForScreenSize(): void {
    const isMobile = window.innerWidth <= 768;
    const rightSection = document.querySelector('.right-section') as HTMLElement;
    
    if (rightSection) {
      if (isMobile) {
        rightSection.style.transform = 'scale(0.8)';
      } else {
        rightSection.style.transform = 'scale(1)';
      }
    }
  }

  /**
   * Add hover effects to interactive elements
   */
  onElementHover(event: MouseEvent, isEntering: boolean): void {
    const target = event.currentTarget as HTMLElement;
    
    if (isEntering) {
      target.style.transform = `${target.style.transform} scale(1.05)`;
      target.style.transition = 'transform 0.3s ease';
    } else {
      target.style.transform = target.style.transform.replace(/scale\([^)]*\)/, '');
    }
  }

  /**
   * Handle scroll-based animations (if needed)
   */
  onScroll(): void {
    const scrollY = window.scrollY;
    const parallaxElements = document.querySelectorAll('.bg-element');
    
    parallaxElements.forEach((el, index) => {
      const element = el as HTMLElement;
      const speed = (index + 1) * 0.1;
      element.style.transform = `translateY(${scrollY * speed}px)`;
    });
  }

  /**
   * Initialize intersection observer for scroll animations
   */
  private initScrollAnimations(): void {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('animate-in');
        }
      });
    });

    // Observe elements that should animate on scroll
    const elementsToAnimate = document.querySelectorAll('.character, .package, .gift-box');
    elementsToAnimate.forEach(el => observer.observe(el));
  }

  /**
   * Utility method to generate random animations
   */
  private getRandomFloat(min: number, max: number): number {
    return Math.random() * (max - min) + min;
  }

  /**
   * Add click ripple effect
   */
  addRippleEffect(event: MouseEvent): void {
    const button = event.currentTarget as HTMLElement;
    const ripple = document.createElement('span');
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;
    
    ripple.style.cssText = `
      position: absolute;
      width: ${size}px;
      height: ${size}px;
      left: ${x}px;
      top: ${y}px;
      background: rgba(255, 255, 255, 0.3);
      border-radius: 50%;
      transform: scale(0);
      animation: ripple 0.6s ease-out;
      pointer-events: none;
    `;
    
    button.style.position = 'relative';
    button.style.overflow = 'hidden';
    button.appendChild(ripple);
    
    setTimeout(() => {
      ripple.remove();
    }, 600);
  }
}