import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PodcastListComponent } from './podcast-list';

describe('PodcastList', () => {
  let component: PodcastListComponent;
  let fixture: ComponentFixture<PodcastListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PodcastListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PodcastListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
