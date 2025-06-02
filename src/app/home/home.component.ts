import { Component, Inject, PLATFORM_ID, AfterViewInit } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements AfterViewInit {

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngAfterViewInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      const counters = document.querySelectorAll('.contador');
      counters.forEach((counter) => {
        const el = counter as HTMLElement;
        const target = +(el.dataset['target'] || '0');
        let count = 0;
        const increment = Math.ceil(target / 100);

        const updateCounter = () => {
          if (count < target) {
            count += increment;
            el.innerText = count.toString();
            requestAnimationFrame(updateCounter);
          } else {
            el.innerText = target.toString();
          }
        };

        updateCounter();
      });
    }
  }
}
