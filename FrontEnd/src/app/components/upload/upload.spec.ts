import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UploadComponent } from './upload';
import { ApiService } from '../../services/api';
import { SignalrService } from '../../services/signalr.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';


describe('UploadComponent', () => {
  let component: UploadComponent;
  let fixture: ComponentFixture<UploadComponent>;

  const apiServiceMock = {
    uploadDocument: () => of({}),
    generatePodcast: () => of({}),
    getCurrentStatus: () => of({ status: 1 })
  };

  const signalrServiceMock = {
    startConnection: jasmine.createSpy('startConnection'),
    onStatusChanged: jasmine.createSpy('onStatusChanged')
  };

  const routerMock = {
    navigate: jasmine.createSpy('navigate')
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UploadComponent, HttpClientTestingModule],
      providers: [
        { provide: ApiService, useValue: apiServiceMock },
        { provide: SignalrService, useValue: signalrServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); 
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});