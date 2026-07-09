import { TestBed } from '@angular/core/testing';

import { Podcast } from './podcast';

describe('Podcast', () => {
  let service: Podcast;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Podcast);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
