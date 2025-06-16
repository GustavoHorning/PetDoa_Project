import { Component, OnInit, OnDestroy, ElementRef, ViewChildren, QueryList, AfterViewInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../app/core/services/auth.service'; // Importe seu AuthService
import { Subscription } from 'rxjs';



@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
    isUserLoggedIn = false;
    private authSubscription!: Subscription;


  stats = {
    animalsAdopted: 4500,
    foodDonatedInKg: 12800,
    totalDonated: 2860000,
    animalsNeutered: 3200
  };

   // Depoimentos com imagens mais autênticas e naturais
  testimonials = [
  {
    name: 'Juliana S.',
    role: 'Doadora',
    quote: 'O Tobi é meu companheiro de todas as horas. O carinho do PetDoa me inspirou a ajudar ainda mais outros animais.',
    imageUrl: 'https://images.pexels.com/photos/4056462/pexels-photo-4056462.jpeg?auto=compress&cs=tinysrgb&w=800'
  },
  {
    name: 'Lucas F.',
    role: 'Doador',
    quote: 'Vi o trabalho do PetDoa nas redes sociais e resolvi ajudar. Saber que minha doação vira ração e cuidados é gratificante.',
    imageUrl: 'https://images.pexels.com/photos/1458925/pexels-photo-1458925.jpeg?auto=compress&cs=tinysrgb&w=800'
  },
  {
    name: 'Carla M.',
    role: 'Adotante e Doadora',
    quote: 'Conheci o PetDoa quando adotei meu gato e hoje sou doadora recorrente. Eles realmente fazem a diferença. A Melhor ONG!',
    imageUrl: 'https://images.pexels.com/photos/5255219/pexels-photo-5255219.jpeg?auto=compress&cs=tinysrgb&w=800'
  }
];


partners = [
  {
    name: 'VetCare Clinic',
    logoUrl: 'https://placehold.co/150x60/00B37E/FFFFFF?text=VetCare+Clinic'
  },
  {
    name: 'Royal Pets',
    logoUrl: 'https://placehold.co/150x60/FFC107/000000?text=Royal+Pets'
  },
  {
    name: 'Happy Paws',
    logoUrl: 'https://placehold.co/150x60/FF5722/FFFFFF?text=Happy+Paws'
  },
  {
    name: 'Animal Support',
    logoUrl: 'https://placehold.co/150x60/3F51B5/FFFFFF?text=Animal+Support'
  },
  {
    name: 'PetShop Legal',
    logoUrl: 'https://placehold.co/150x60/9C27B0/FFFFFF?text=PetShop+Legal'
  }
];


  @ViewChildren('counter') counters!: QueryList<ElementRef>;
  private observer!: IntersectionObserver;

  constructor(private authService: AuthService,  @Inject(PLATFORM_ID) private platformId: object) {}

  ngOnInit(): void {
    this.authSubscription = this.authService.isLoggedIn$.subscribe(isLoggedIn => {
      this.isUserLoggedIn = isLoggedIn;
    });
  }

  ngAfterViewInit(): void {
    // Verificamos se estamos no navegador antes de usar a API do browser
    if (isPlatformBrowser(this.platformId)) {
      const options = { root: null, threshold: 0.5 };
      this.observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            const element = entry.target as HTMLElement;
            const target = +element.getAttribute('data-target')!;
            this.animateCount(element, target);
            this.observer.unobserve(element);
          }
        });
      }, options);

      setTimeout(() => {
          if (this.counters) {
              this.counters.forEach(counter => {
                  if(counter && counter.nativeElement) {
                      this.observer.observe(counter.nativeElement);
                  }
              });
          }
      }, 0);
    }
  }

  ngOnDestroy(): void {
    if (isPlatformBrowser(this.platformId) && this.observer) {
      this.observer.disconnect();
    }
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  animateCount(element: HTMLElement, target: number): void {
    let current = 0;
    const duration = 2000;
    const stepTime = 20;
    const steps = duration / stepTime;
    const increment = target / steps;
    const timer = setInterval(() => {
      current += increment;
      if (current >= target) {
        clearInterval(timer);
        element.innerText = target.toLocaleString('pt-BR');
      } else {
        element.innerText = Math.ceil(current).toLocaleString('pt-BR');
      }
    }, stepTime);
  }

  scrollToSection(sectionId: string): void {
    document.getElementById(sectionId)?.scrollIntoView({ behavior: 'smooth' });
  }
}