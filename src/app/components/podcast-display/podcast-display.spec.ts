import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PodcastDisplayComponent } from './podcast-display';

describe('PodcastDisplayComponent', () => {
  let component: PodcastDisplayComponent;
  let fixture: ComponentFixture<PodcastDisplayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      // بنضيف المكون هنا لأنه Standalone
      imports: [PodcastDisplayComponent],
      // لازم نوفر الـ Router لأننا مستخدمين RouterLink في الـ HTML
      providers: [provideRouter([])]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PodcastDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // لتفعيل الـ Data Binding الأولي
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});