const test = require('node:test');
const assert = require('node:assert');
const TD = require('../js/core.js');

test('formatFireRate labels seconds and marks faster-is-lower', () => {
  assert.strictEqual(TD.formatFireRate(1.5), '1.5s / shot');
  assert.strictEqual(TD.formatFireRate(0.25), '0.25s / shot');
});

test('matchesSearch is case-insensitive substring on name', () => {
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, 'can'), true);
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, 'TOWER'), true);
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, 'frost'), false);
  assert.strictEqual(TD.matchesSearch({ name: 'Cannon Tower' }, ''), true);
});

test('towerMatchesFilters checks effect and targeting (empty = pass)', () => {
  const t = { effect: { name: 'AOE Impact' }, targeting: 'Strongest' };
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: '', targeting: '' }), true);
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: 'AOE Impact', targeting: '' }), true);
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: 'DOT', targeting: '' }), false);
  assert.strictEqual(TD.towerMatchesFilters(t, { effect: '', targeting: 'First' }), false);
});

test('ruleMatchesFilters passes when rule has the chosen segment', () => {
  const r = { segments: ['Enemy', 'Tower'] };
  assert.strictEqual(TD.ruleMatchesFilters(r, { segment: '' }), true);
  assert.strictEqual(TD.ruleMatchesFilters(r, { segment: 'Tower' }), true);
  assert.strictEqual(TD.ruleMatchesFilters(r, { segment: 'Economy' }), false);
});

test('enemyMatchesFilters checks type', () => {
  const e = { type: 'fast' };
  assert.strictEqual(TD.enemyMatchesFilters(e, { type: '' }), true);
  assert.strictEqual(TD.enemyMatchesFilters(e, { type: 'fast' }), true);
  assert.strictEqual(TD.enemyMatchesFilters(e, { type: 'tank' }), false);
});

test('uniqueValues collects sorted distinct values via accessor', () => {
  const items = [{ targeting: 'First' }, { targeting: 'Last' }, { targeting: 'First' }];
  assert.deepStrictEqual(TD.uniqueValues(items, x => x.targeting), ['First', 'Last']);
});
