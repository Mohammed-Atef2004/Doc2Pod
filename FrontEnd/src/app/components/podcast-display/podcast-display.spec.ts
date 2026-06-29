import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PodcastDisplayComponent } from './podcast-display';
import { ApiService } from '../../services/api';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

describe('PodcastDisplayComponent', () => {
  let component: PodcastDisplayComponent;
  let fixture: ComponentFixture<PodcastDisplayComponent>;
  let mockApiService: any;

  beforeEach(async () => {
    mockApiService = jasmine.createSpyObj('ApiService', ['getPodcast']);
    mockApiService.getPodcast.and.returnValue(of(new Blob()));

    await TestBed.configureTestingModule({
      imports: [PodcastDisplayComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      
        { provide: ApiService, useValue: mockApiService },

        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: { get: () => '123' } }
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PodcastDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); 
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getPodcast on init', () => {
    expect(mockApiService.getPodcast).toHaveBeenCalledWith('123');
  });
});