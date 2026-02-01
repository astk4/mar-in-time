import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SideInfo } from './side-info';

describe('SideInfo', () => {
  let component: SideInfo;
  let fixture: ComponentFixture<SideInfo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SideInfo]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SideInfo);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
