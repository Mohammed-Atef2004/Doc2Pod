import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailUpdateConfirm } from './email-update-confirm';

describe('EmailUpdateConfirm', () => {
  let component: EmailUpdateConfirm;
  let fixture: ComponentFixture<EmailUpdateConfirm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailUpdateConfirm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailUpdateConfirm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
