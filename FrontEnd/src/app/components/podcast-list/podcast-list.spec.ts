import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PodcastListComponent } from './podcast-list';
import { ApiService } from '../../services/api';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('PodcastList', () => {
  let component: PodcastListComponent;
  let fixture: ComponentFixture<PodcastListComponent>;
  let mockPodcastService: any;

  beforeEach(async () => {
    mockPodcastService = jasmine.createSpyObj('PodcastService', ['getUserPodcasts']);
    mockPodcastService.getUserPodcasts.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [PodcastListComponent],
      providers: [
        { provide: ApiService , useValue: mockPodcastService }, 
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PodcastListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
