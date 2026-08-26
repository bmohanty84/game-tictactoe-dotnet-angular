import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScoreboardComponent } from './scoreboard.component';

describe('ScoreboardComponent', () => {
  let fixture: ComponentFixture<ScoreboardComponent>;
  let component: ScoreboardComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({ declarations: [ScoreboardComponent] });
    fixture = TestBed.createComponent(ScoreboardComponent);
    component = fixture.componentInstance;
  });

  it('renders the provided win/draw counts', () => {
    component.scoreboard = { xWins: 3, oWins: 1, draws: 2 };
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('3');
    expect(text).toContain('1');
    expect(text).toContain('2');
  });

  it('emits resetScoreboard when the reset button is clicked', () => {
    fixture.detectChanges();
    spyOn(component.resetScoreboard, 'emit');

    component.onReset();

    expect(component.resetScoreboard.emit).toHaveBeenCalled();
  });
});
