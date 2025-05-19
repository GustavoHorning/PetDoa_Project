import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InstitutionCardComponent } from './institution-card.component';

describe('InstitutionCardComponent', () => {
  let component: InstitutionCardComponent;
  let fixture: ComponentFixture<InstitutionCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InstitutionCardComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InstitutionCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
