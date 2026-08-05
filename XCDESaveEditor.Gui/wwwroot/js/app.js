'use strict';

/* ---------------------------------------------------------------------- */
/* Backend bridge                                                         */
/* ---------------------------------------------------------------------- */

let pendingResolve = null;

window.external.receiveMessage((message) => {
  const response = JSON.parse(message);
  if (pendingResolve) {
    const resolve = pendingResolve;
    pendingResolve = null;
    resolve(response);
  }
});

function callBackend(action, data) {
  return new Promise((resolve) => {
    pendingResolve = resolve;
    window.external.sendMessage(JSON.stringify({ action: action, data: data || {} }));
  });
}

/* ---------------------------------------------------------------------- */
/* State                                                                  */
/* ---------------------------------------------------------------------- */

const state = {
  loaded: false,
  filePath: '',
  backupCreated: false,
  snapshot: null,
  armorCategory: 'headArmor',
  gemCategory: 'gem',
  itemCategory: 'collectable',
  statsCharacterId: 1,
};

const EQUIP_BOX_KEYS = {
  weapon: 'weapons',
  headArmor: 'headArmor',
  torsoArmor: 'torsoArmor',
  armArmor: 'armArmor',
  legArmor: 'legArmor',
  footArmor: 'footArmor',
};

const ITEM_BOX_KEYS = {
  collectable: 'collectables',
  material: 'materials',
  keyItem: 'keyItems',
  artsManual: 'artsManuals',
};

const CATEGORY_LABELS = {
  weapon: 'weapon',
  headArmor: 'head armor piece',
  torsoArmor: 'torso armor piece',
  armArmor: 'arm armor piece',
  legArmor: 'leg armor piece',
  footArmor: 'foot armor piece',
  collectable: 'collectable',
  material: 'material',
  keyItem: 'key item',
  artsManual: 'arts manual',
};

/* ---------------------------------------------------------------------- */
/* Toast                                                                  */
/* ---------------------------------------------------------------------- */

let toastTimer = null;

function showToast(message, isError) {
  const toast = document.getElementById('toast');
  toast.textContent = message;
  toast.className = 'toast ' + (isError ? 'error' : 'success');
  toast.hidden = false;

  if (toastTimer) {
    window.clearTimeout(toastTimer);
  }
  toastTimer = window.setTimeout(() => {
    toast.hidden = true;
  }, 4000);
}

/* ---------------------------------------------------------------------- */
/* Item name lookups                                                      */
/* ---------------------------------------------------------------------- */

function getItemName(categoryKey, id) {
  const table = ITEM_NAMES[categoryKey];
  if (!table) {
    return 'ID ' + id;
  }
  const name = table[String(id)];
  return name || ('Unnamed #' + id);
}

function isUnnamedName(name) {
  return typeof name === 'string' && name.indexOf('Unnamed #') === 0;
}

function sortedIds(categoryKey) {
  const table = ITEM_NAMES[categoryKey] || {};
  return Object.keys(table).map(Number).sort((a, b) => a - b);
}

function getGemSlotInfo(itemId) {
  const info = ITEM_NAMES.gemSlotInfo ? ITEM_NAMES.gemSlotInfo[String(itemId)] : null;
  if (!info) {
    return null;
  }
  return info;
}

/* ---------------------------------------------------------------------- */
/* Item picker (datalist based, supports the large id lists)              */
/* ---------------------------------------------------------------------- */

function populateDatalist(datalistElement, categoryKey) {
  datalistElement.innerHTML = '';
  const ids = sortedIds(categoryKey);
  const fragment = document.createDocumentFragment();
  for (const id of ids) {
    const name = getItemName(categoryKey, id);
    const option = document.createElement('option');
    option.value = id + ' - ' + name;
    fragment.appendChild(option);
  }
  datalistElement.appendChild(fragment);
}

function parseIdFromPickerText(text) {
  const match = /^(\d+)/.exec(String(text).trim());
  return match ? parseInt(match[1], 10) : NaN;
}

function setPickerValueById(inputElement, categoryKey, id) {
  if (id === undefined || id === null || id === 0) {
    inputElement.value = '';
    return;
  }
  inputElement.value = id + ' - ' + getItemName(categoryKey, id);
}

/* ---------------------------------------------------------------------- */
/* Top bar                                                                */
/* ---------------------------------------------------------------------- */

function renderTopbar() {
  const fileStatus = document.getElementById('fileStatus');
  const fileDot = document.getElementById('fileDot');
  const btnSave = document.getElementById('btnSave');
  const btnSaveAs = document.getElementById('btnSaveAs');
  const backupStatus = document.getElementById('backupStatus');

  if (state.loaded) {
    fileStatus.textContent = state.filePath;
    fileDot.classList.add('loaded');
    btnSave.disabled = false;
    btnSaveAs.disabled = false;
  } else {
    fileStatus.textContent = 'No file loaded';
    fileDot.classList.remove('loaded');
    btnSave.disabled = true;
    btnSaveAs.disabled = true;
  }

  backupStatus.textContent = state.backupCreated ? 'created' : 'not created';

  document.getElementById('emptyState').hidden = state.loaded;
  document.getElementById('panels').hidden = !state.loaded;
}

/* ---------------------------------------------------------------------- */
/* Party panel                                                            */
/* ---------------------------------------------------------------------- */

function characterName(characterId) {
  const member = state.snapshot.partyMembers.find((m) => m.characterId === characterId);
  return member ? member.characterName.replace('_', ' ') : ('Character ' + characterId);
}

function renderPartyRoster() {
  const container = document.getElementById('partyRoster');
  container.innerHTML = '';

  const roster = state.snapshot.partyCharacterIds;

  if (roster.length === 0) {
    const empty = document.createElement('span');
    empty.className = 'chip-empty';
    empty.textContent = 'No characters currently in the active party.';
    container.appendChild(empty);
  }

  for (const characterId of roster) {
    const chip = document.createElement('span');
    chip.className = 'chip';

    const label = document.createElement('span');
    label.textContent = characterName(characterId);
    chip.appendChild(label);

    const removeButton = document.createElement('button');
    removeButton.textContent = '\u00D7';
    removeButton.title = 'Remove from party';
    removeButton.addEventListener('click', async () => {
      const response = await callBackend('removeCharacter', { characterId: characterId });
      applyResponse(response);
    });
    chip.appendChild(removeButton);

    container.appendChild(chip);
  }

  const addSelect = document.getElementById('addCharacterSelect');
  addSelect.innerHTML = '';
  for (const member of state.snapshot.partyMembers) {
    const option = document.createElement('option');
    option.value = member.characterId;
    option.textContent = member.characterId + ' - ' + member.characterName.replace('_', ' ');
    addSelect.appendChild(option);
  }
}

function renderCharacterStatsForm() {
  const select = document.getElementById('statsCharacterSelect');
  if (select.options.length === 0) {
    for (const member of state.snapshot.partyMembers) {
      const option = document.createElement('option');
      option.value = member.characterId;
      option.textContent = member.characterId + ' - ' + member.characterName.replace('_', ' ');
      select.appendChild(option);
    }
    select.value = String(state.statsCharacterId);
  }

  const member = state.snapshot.partyMembers.find((m) => m.characterId === state.statsCharacterId);
  const container = document.getElementById('characterStatsForm');
  container.innerHTML = '';

  if (!member) {
    container.innerHTML = '<p class="empty-row">Character not found in save data.</p>';
    return;
  }

  const grid = document.createElement('div');
  grid.className = 'form-grid';

  const levelField = buildNumberField('Level', 'statLevel', member.level, 0, 99);
  const apField = buildNumberField('AP', 'statAp', member.ap, 0, 999999999);
  const expField = buildNumberField('EXP (read only)', 'statExp', member.exp, 0, 999999999, true);
  const affinityField = buildNumberField('Affinity coins (read only)', 'statAffinity', member.affinityCoins, 0, 999999999, true);

  grid.appendChild(levelField.wrap);
  grid.appendChild(apField.wrap);
  grid.appendChild(expField.wrap);
  grid.appendChild(affinityField.wrap);

  container.appendChild(grid);

  const actions = document.createElement('div');
  actions.className = 'form-actions';

  const saveLevelButton = document.createElement('button');
  saveLevelButton.className = 'btn btn-secondary';
  saveLevelButton.textContent = 'Update level';
  saveLevelButton.addEventListener('click', async () => {
    const level = parseInt(levelField.input.value, 10) || 0;
    const response = await callBackend('setCharacterLevel', { characterId: member.characterId, level: level });
    applyResponse(response);
  });
  actions.appendChild(saveLevelButton);

  const saveApButton = document.createElement('button');
  saveApButton.className = 'btn btn-secondary';
  saveApButton.textContent = 'Update AP';
  saveApButton.addEventListener('click', async () => {
    const ap = parseInt(apField.input.value, 10) || 0;
    const response = await callBackend('setCharacterAp', { characterId: member.characterId, ap: ap });
    applyResponse(response);
  });
  actions.appendChild(saveApButton);

  container.appendChild(actions);

  const artsTitle = document.createElement('h3');
  artsTitle.className = 'subsection-title';
  artsTitle.style.marginTop = '20px';
  artsTitle.textContent = 'Arts (9 slots, slot 1 is the talent art)';
  container.appendChild(artsTitle);

  container.appendChild(buildArtsGrid(member, 'arts', false));

  const monadoTitle = document.createElement('h3');
  monadoTitle.className = 'subsection-title';
  monadoTitle.style.marginTop = '16px';
  monadoTitle.textContent = 'Arts while Monado is active (Shulk only, ignored for others)';
  container.appendChild(monadoTitle);

  container.appendChild(buildArtsGrid(member, 'monadoArts', true));

  container.appendChild(buildAdvancedBlock(member));
}

function buildNumberField(labelText, inputId, value, min, max, readOnly) {
  const wrap = document.createElement('div');
  wrap.className = 'form-field';

  const label = document.createElement('label');
  label.setAttribute('for', inputId);
  label.textContent = labelText;
  wrap.appendChild(label);

  const input = document.createElement('input');
  input.className = 'input input-number';
  input.type = 'number';
  input.id = inputId;
  input.min = String(min);
  input.max = String(max);
  input.value = String(value);
  if (readOnly) {
    input.disabled = true;
  }
  wrap.appendChild(input);

  return { wrap: wrap, input: input };
}

function buildArtsGrid(member, fieldName, useMonadoSet) {
  const grid = document.createElement('div');
  grid.className = 'arts-grid';

  const values = member[fieldName];

  for (let slotIndex = 0; slotIndex < values.length; slotIndex++) {
    const slotWrap = document.createElement('div');
    slotWrap.className = 'arts-slot';

    const label = document.createElement('label');
    label.textContent = slotIndex === 0 ? 'Slot 1 (talent)' : ('Slot ' + (slotIndex + 1));
    slotWrap.appendChild(label);

    const input = document.createElement('input');
    input.className = 'input';
    input.setAttribute('list', 'artsDatalist');
    setPickerValueById(input, 'arts', values[slotIndex]);
    input.addEventListener('change', async () => {
      const artId = parseIdFromPickerText(input.value);
      if (isNaN(artId)) {
        showToast('Could not read an art ID from that entry.', true);
        return;
      }
      const response = await callBackend('setCharacterArtSlot', {
        characterId: member.characterId,
        slotIndex: slotIndex,
        artId: artId,
        useMonadoSet: useMonadoSet,
      });
      applyResponse(response);
    });
    slotWrap.appendChild(input);

    grid.appendChild(slotWrap);
  }

  return grid;
}

function buildAdvancedBlock(member) {
  const wrapper = document.createElement('div');

  const toggle = document.createElement('button');
  toggle.className = 'details-toggle';
  toggle.textContent = 'Advanced: unidentified data blocks (experimental, this is where an undiscovered TP-like value might live)';
  wrapper.appendChild(toggle);

  const block = document.createElement('div');
  block.className = 'advanced-block';

  const blocks = [
    { key: 'unk1', label: 'Unk_1 (4 bytes)', value: member.unk1Hex },
    { key: 'unk2', label: 'Unk_2 (4 bytes)', value: member.unk2Hex },
    { key: 'unk3', label: 'Unk_3 (12 bytes)', value: member.unk3Hex },
    { key: 'unk4', label: 'Unk_4 (37 bytes)', value: member.unk4Hex },
    { key: 'unk5', label: 'Unk_5 (64 bytes)', value: member.unk5Hex },
  ];

  for (const blockInfo of blocks) {
    const row = document.createElement('div');
    row.className = 'advanced-row';

    const label = document.createElement('span');
    label.className = 'field-label';
    label.textContent = blockInfo.label;
    row.appendChild(label);

    const input = document.createElement('input');
    input.className = 'input input-hex';
    input.value = blockInfo.value;
    row.appendChild(input);

    const applyButton = document.createElement('button');
    applyButton.className = 'btn btn-small btn-secondary';
    applyButton.textContent = 'Apply';
    applyButton.addEventListener('click', async () => {
      const response = await callBackend('setCharacterUnknownBlock', {
        characterId: member.characterId,
        blockName: blockInfo.key,
        hexValue: input.value.trim(),
      });
      applyResponse(response);
    });
    row.appendChild(applyButton);

    block.appendChild(row);
  }

  toggle.addEventListener('click', () => {
    block.classList.toggle('open');
  });

  wrapper.appendChild(block);
  return wrapper;
}

/* ---------------------------------------------------------------------- */
/* Equip editor (weapons and the five armor categories)                   */
/* ---------------------------------------------------------------------- */

let equipEditState = { weapon: null, headArmor: null, torsoArmor: null, armArmor: null, legArmor: null, footArmor: null };

function renderEquipEditor(containerId, category) {
  const container = document.getElementById(containerId);
  container.innerHTML = '';

  const boxKey = EQUIP_BOX_KEYS[category];
  const entries = state.snapshot[boxKey];

  const table = document.createElement('table');
  table.className = 'slot-table';
  table.innerHTML = '<thead><tr><th>Slot</th><th>Item</th><th>Qty</th><th>Gem sockets</th><th></th></tr></thead>';
  const tbody = document.createElement('tbody');

  if (entries.length === 0) {
    const row = document.createElement('tr');
    row.innerHTML = '<td colspan="5" class="empty-row">No items in this box.</td>';
    tbody.appendChild(row);
  }

  for (const entry of entries) {
    const row = document.createElement('tr');

    const name = getItemName(category, entry.itemId);
    const unnamed = isUnnamedName(name);
    const gemInfo = getGemSlotInfo(entry.itemId);
    const fixed = gemInfo && gemInfo.fixedSkills && gemInfo.fixedSkills.length > 0;
    const fixedBadge = fixed ? '<span class="badge badge-fixed" title="Fixed sockets: ' + escapeHtml(gemInfo.fixedSkills.join(', ')) + '">fixed sockets</span>' : '';

    row.innerHTML =
      '<td class="slot-id">' + entry.slotIndex + '</td>' +
      '<td><span class="item-name' + (unnamed ? ' unnamed' : '') + '">' + escapeHtml(name) + '</span>' +
      '<span class="slot-id"> (#' + entry.itemId + ')</span>' +
      (unnamed ? '<span class="badge badge-debug">no game name</span>' : '') +
      fixedBadge + '</td>' +
      '<td>' + entry.quantity + '</td>' +
      '<td>' + renderSockets(entry) + '</td>' +
      '<td></td>';

    const actionsCell = row.lastElementChild;

    const editButton = document.createElement('button');
    editButton.className = 'btn btn-small';
    editButton.textContent = 'Edit';
    editButton.style.marginRight = '6px';
    editButton.addEventListener('click', () => {
      equipEditState[category] = entry;
      renderEquipForm(containerId, category);
    });
    actionsCell.appendChild(editButton);

    const removeButton = document.createElement('button');
    removeButton.className = 'btn btn-small btn-danger';
    removeButton.textContent = 'Remove';
    removeButton.addEventListener('click', async () => {
      const response = await callBackend('removeEquip', { category: category, slotIndex: entry.slotIndex });
      applyResponse(response);
    });
    actionsCell.appendChild(removeButton);

    tbody.appendChild(row);
  }

  table.appendChild(tbody);
  container.appendChild(table);

  renderEquipForm(containerId, category);
}

function renderSockets(entry) {
  const gemInfo = getGemSlotInfo(entry.itemId);
  const fixedSkills = gemInfo && gemInfo.fixedSkills ? gemInfo.fixedSkills : [];

  let html = '<span class="sockets">';
  for (let i = 0; i < entry.gemSlots; i++) {
    const gemId = [entry.gem1Id, entry.gem2Id, entry.gem3Id][i];
    const fixedName = fixedSkills[i];
    const tooltip = fixedName ? ('fixed: ' + fixedName) : (gemId ? getItemName('gem', gemId) : 'empty');
    const filled = fixedName || gemId;
    html += '<span class="socket' + (filled ? ' filled' : '') + '" title="' + escapeHtml(tooltip) + '"></span>';
  }
  html += '</span>';
  return html;
}

function renderEquipForm(containerId, category) {
  const container = document.getElementById(containerId);
  const existingForm = container.querySelector('.editor-form');
  if (existingForm) {
    existingForm.remove();
  }

  const editing = equipEditState[category];
  const idPrefix = containerId + '-';

  const form = document.createElement('div');
  form.className = 'editor-form';

  const title = document.createElement('p');
  title.className = 'editor-form-title';
  title.textContent = editing ? ('Editing slot ' + editing.slotIndex) : ('Add a new ' + CATEGORY_LABELS[category]);
  form.appendChild(title);

  const grid = document.createElement('div');
  grid.className = 'form-grid';

  const slotField = document.createElement('div');
  slotField.className = 'form-field';
  slotField.innerHTML = '<label>Slot index (0-499, blank = first free)</label>';
  const slotInput = document.createElement('input');
  slotInput.className = 'input';
  slotInput.type = 'number';
  slotInput.min = '0';
  slotInput.max = '499';
  slotInput.id = idPrefix + 'equipSlotIndex';
  if (editing) {
    slotInput.value = String(editing.slotIndex);
    slotInput.disabled = true;
  }
  slotField.appendChild(slotInput);
  grid.appendChild(slotField);

  const itemField = document.createElement('div');
  itemField.className = 'form-field';
  itemField.innerHTML = '<label>Item</label>';
  const itemInput = document.createElement('input');
  itemInput.className = 'input';
  itemInput.id = idPrefix + 'equipItemId';
  itemInput.setAttribute('list', idPrefix + 'datalist-' + category);
  if (editing) {
    setPickerValueById(itemInput, category, editing.itemId);
  }
  itemField.appendChild(itemInput);
  const datalist = document.createElement('datalist');
  datalist.id = idPrefix + 'datalist-' + category;
  populateDatalist(datalist, category);
  itemField.appendChild(datalist);
  grid.appendChild(itemField);

  const gemTypeNote = document.createElement('p');
  gemTypeNote.className = 'panel-hint';
  gemTypeNote.style.margin = '0 0 14px';

  const quantityField = buildNumberField('Quantity', idPrefix + 'equipQuantity', editing ? editing.quantity : 1, 0, 65535);
  grid.appendChild(quantityField.wrap);

  const gemSlotsField = buildNumberField('Gem sockets (0-3)', idPrefix + 'equipGemSlots', editing ? editing.gemSlots : 0, 0, 3);
  grid.appendChild(gemSlotsField.wrap);

  const gemInputs = [];
  const gemDatalist = document.createElement('datalist');
  gemDatalist.id = idPrefix + 'datalist-gem';
  populateDatalist(gemDatalist, 'gem');
  grid.appendChild(gemDatalist);

  for (let i = 1; i <= 3; i++) {
    const gemField = document.createElement('div');
    gemField.className = 'form-field';
    gemField.innerHTML = '<label>Gem in socket ' + i + '</label>';
    const gemInput = document.createElement('input');
    gemInput.className = 'input';
    gemInput.id = idPrefix + 'equipGem' + i;
    gemInput.setAttribute('list', idPrefix + 'datalist-gem');
    gemInput.placeholder = 'pick a gem, blank = empty';
    if (editing) {
      const gemValue = [editing.gem1Id, editing.gem2Id, editing.gem3Id][i - 1];
      setPickerValueById(gemInput, 'gem', gemValue);
    }
    gemField.appendChild(gemInput);
    grid.appendChild(gemField);
    gemInputs.push(gemInput);
  }

  form.appendChild(grid);
  form.appendChild(gemTypeNote);

  function updateGemSlotTypeUI() {
    const currentItemId = parseIdFromPickerText(itemInput.value);
    const info = isNaN(currentItemId) ? null : getGemSlotInfo(currentItemId);
    const fixed = info && info.fixedSkills && info.fixedSkills.length > 0;

    if (fixed) {
      gemTypeNote.textContent = 'In the game\'s own menu, this item shows fixed gem sockets: ' + info.fixedSkills.join(', ') + '. The in-game menu will not let you change them, but a save editor can, and the game accepts the result. Where a socket below is still empty, it has been pre-filled with the game\'s default for this item; change it if you want something else.';
      gemTypeNote.classList.add('fixed-slot-note');
    } else {
      gemTypeNote.textContent = 'This item has free gem sockets, any gem set below can be freely changed in-game too.';
      gemTypeNote.classList.remove('fixed-slot-note');
    }

    if (fixed && info.fixedGemIds) {
      for (let i = 0; i < gemInputs.length; i++) {
        const currentValue = parseIdFromPickerText(gemInputs[i].value);
        const alreadySet = !isNaN(currentValue) && currentValue !== 0;
        const defaultGemId = info.fixedGemIds[i];
        if (!alreadySet && defaultGemId) {
          setPickerValueById(gemInputs[i], 'gem', defaultGemId);
        }
      }
      const currentSlots = parseInt(gemSlotsField.input.value, 10) || 0;
      if (currentSlots === 0 && info.slots) {
        gemSlotsField.input.value = String(info.slots);
      }
    }
  }

  itemInput.addEventListener('input', updateGemSlotTypeUI);
  itemInput.addEventListener('change', updateGemSlotTypeUI);
  updateGemSlotTypeUI();

  const actions = document.createElement('div');
  actions.className = 'form-actions';

  const submitButton = document.createElement('button');
  submitButton.className = 'btn btn-primary';
  submitButton.textContent = editing ? 'Save changes' : 'Add item';
  submitButton.addEventListener('click', async () => {
    const itemId = parseIdFromPickerText(itemInput.value);
    if (isNaN(itemId)) {
      showToast('Enter or pick a valid item ID.', true);
      return;
    }

    const payload = {
      category: category,
      itemId: itemId,
      quantity: parseInt(quantityField.input.value, 10) || 0,
      gemSlots: parseInt(gemSlotsField.input.value, 10) || 0,
      gem1Id: parseIdFromPickerText(gemInputs[0].value) || 0,
      gem2Id: parseIdFromPickerText(gemInputs[1].value) || 0,
      gem3Id: parseIdFromPickerText(gemInputs[2].value) || 0,
    };

    if (editing) {
      payload.slotIndex = editing.slotIndex;
    } else if (slotInput.value !== '') {
      payload.slotIndex = parseInt(slotInput.value, 10);
    }

    const response = await callBackend('addOrUpdateEquip', payload);
    if (response.success) {
      equipEditState[category] = null;
    }
    applyResponse(response);
  });
  actions.appendChild(submitButton);

  if (editing) {
    const cancelButton = document.createElement('button');
    cancelButton.className = 'btn btn-ghost';
    cancelButton.textContent = 'Cancel';
    cancelButton.addEventListener('click', () => {
      equipEditState[category] = null;
      renderEquipForm(containerId, category);
    });
    actions.appendChild(cancelButton);
  }

  form.appendChild(actions);
  container.appendChild(form);
}

/* ---------------------------------------------------------------------- */
/* Gem / crystal editor                                                   */
/* ---------------------------------------------------------------------- */

let gemEditState = { gem: null, crystal: null };

function renderGemEditor(category) {
  const container = document.getElementById('gemEditor');
  container.innerHTML = '';

  const boxKey = category === 'gem' ? 'gems' : 'crystals';
  const entries = state.snapshot[boxKey];

  const table = document.createElement('table');
  table.className = 'slot-table';
  table.innerHTML = '<thead><tr><th>Slot</th><th>Item</th><th>Base crystal</th><th>Rank</th><th>Element</th><th>Qty</th><th></th></tr></thead>';
  const tbody = document.createElement('tbody');

  if (entries.length === 0) {
    const row = document.createElement('tr');
    row.innerHTML = '<td colspan="7" class="empty-row">No items in this box.</td>';
    tbody.appendChild(row);
  }

  for (const entry of entries) {
    const row = document.createElement('tr');
    const itemName = getItemName(category, entry.itemId);
    const unnamed = isUnnamedName(itemName);
    const crystalName = getItemName('crystalName', entry.crystalNameId);

    row.innerHTML =
      '<td class="slot-id">' + entry.slotIndex + '</td>' +
      '<td><span class="item-name' + (unnamed ? ' unnamed' : '') + '">' + escapeHtml(itemName) + '</span>' +
      '<span class="slot-id"> (#' + entry.itemId + ')</span></td>' +
      '<td>' + escapeHtml(crystalName) + '</td>' +
      '<td>' + entry.rank + '</td>' +
      '<td>' + elementName(entry.element) + '</td>' +
      '<td>' + entry.quantity + '</td>' +
      '<td></td>';

    const actionsCell = row.lastElementChild;

    const editButton = document.createElement('button');
    editButton.className = 'btn btn-small';
    editButton.textContent = 'Edit';
    editButton.style.marginRight = '6px';
    editButton.addEventListener('click', () => {
      gemEditState[category] = entry;
      renderGemForm(category);
    });
    actionsCell.appendChild(editButton);

    const removeButton = document.createElement('button');
    removeButton.className = 'btn btn-small btn-danger';
    removeButton.textContent = 'Remove';
    removeButton.addEventListener('click', async () => {
      const response = await callBackend('removeGem', { category: category, slotIndex: entry.slotIndex });
      applyResponse(response);
    });
    actionsCell.appendChild(removeButton);

    tbody.appendChild(row);
  }

  table.appendChild(tbody);
  container.appendChild(table);

  renderGemForm(category);
}

const ELEMENT_NAMES = { 0: 'White', 1: 'White', 2: 'White', 3: 'White', 4: 'Fire', 5: 'Water', 6: 'Electric', 7: 'Ice', 8: 'Wind', 9: 'Earth', 10: 'White / mixed' };

function elementName(value) {
  return ELEMENT_NAMES[value] !== undefined ? ELEMENT_NAMES[value] : String(value);
}

function renderGemForm(category) {
  const container = document.getElementById('gemEditor');
  const existingForm = container.querySelector('.editor-form');
  if (existingForm) {
    existingForm.remove();
  }

  const editing = gemEditState[category];

  const form = document.createElement('div');
  form.className = 'editor-form';

  const title = document.createElement('p');
  title.className = 'editor-form-title';
  title.textContent = editing ? ('Editing slot ' + editing.slotIndex) : ('Add a new ' + category);
  form.appendChild(title);

  const hint = document.createElement('p');
  hint.className = 'panel-hint';
  hint.style.marginBottom = '14px';
  hint.textContent = 'The item name comes from the skill it grants plus its rank (for example "Unbeatable VI"). Where two different items would otherwise share the same name, a value or ID is appended to tell them apart.';
  form.appendChild(hint);

  const grid = document.createElement('div');
  grid.className = 'form-grid';

  const slotField = document.createElement('div');
  slotField.className = 'form-field';
  slotField.innerHTML = '<label>Slot index (0-499, blank = first free)</label>';
  const slotInput = document.createElement('input');
  slotInput.className = 'input';
  slotInput.type = 'number';
  slotInput.min = '0';
  slotInput.max = '499';
  slotInput.id = 'gemSlotIndex';
  if (editing) {
    slotInput.value = String(editing.slotIndex);
    slotInput.disabled = true;
  }
  slotField.appendChild(slotInput);
  grid.appendChild(slotField);

  const itemField = document.createElement('div');
  itemField.className = 'form-field';
  itemField.innerHTML = '<label>Item</label>';
  const itemInput = document.createElement('input');
  itemInput.className = 'input';
  itemInput.id = 'gemItemId';
  itemInput.setAttribute('list', 'datalist-' + category);
  if (editing) {
    setPickerValueById(itemInput, category, editing.itemId);
  }
  itemField.appendChild(itemInput);
  const itemDatalist = document.createElement('datalist');
  itemDatalist.id = 'datalist-' + category;
  populateDatalist(itemDatalist, category);
  itemField.appendChild(itemDatalist);
  grid.appendChild(itemField);

  const nameField = document.createElement('div');
  nameField.className = 'form-field';
  nameField.innerHTML = '<label>Base crystal name</label>';
  const nameInput = document.createElement('input');
  nameInput.className = 'input';
  nameInput.id = 'gemCrystalName';
  nameInput.setAttribute('list', 'datalist-crystalName');
  if (editing) {
    setPickerValueById(nameInput, 'crystalName', editing.crystalNameId);
  }
  nameField.appendChild(nameInput);
  const datalist = document.createElement('datalist');
  datalist.id = 'datalist-crystalName';
  populateDatalist(datalist, 'crystalName');
  nameField.appendChild(datalist);
  grid.appendChild(nameField);

  const rankField = buildNumberField('Rank', 'gemRank', editing ? editing.rank : 1, 1, 6);
  const elementField = buildNumberField('Element (4 Fire, 5 Water, 6 Electric, 7 Ice, 8 Wind, 9 Earth)', 'gemElement', editing ? editing.element : 4, 0, 10);
  const quantityField = buildNumberField('Quantity', 'gemQuantity', editing ? editing.quantity : 1, 0, 65535);
  grid.appendChild(rankField.wrap);
  grid.appendChild(elementField.wrap);
  grid.appendChild(quantityField.wrap);

  form.appendChild(grid);

  const buffTitle = document.createElement('p');
  buffTitle.className = 'editor-form-title';
  buffTitle.style.marginTop = '10px';
  buffTitle.textContent = 'Buffs (numeric buff ID and value, gems only use buff 1)';
  form.appendChild(buffTitle);

  const buffGrid = document.createElement('div');
  buffGrid.className = 'form-grid';
  const buffFields = [];
  for (let i = 1; i <= 4; i++) {
    const buffIdField = buildNumberField('Buff ' + i + ' ID', 'gemBuff' + i + 'Id', editing ? editing['buff' + i + 'Id'] : 0, 0, 65535);
    const buffValueField = buildNumberField('Buff ' + i + ' value', 'gemBuff' + i + 'Value', editing ? editing['buff' + i + 'Value'] : 0, 0, 65535);
    buffGrid.appendChild(buffIdField.wrap);
    buffGrid.appendChild(buffValueField.wrap);
    buffFields.push({ id: buffIdField, value: buffValueField });
  }
  form.appendChild(buffGrid);

  const actions = document.createElement('div');
  actions.className = 'form-actions';

  const submitButton = document.createElement('button');
  submitButton.className = 'btn btn-primary';
  submitButton.textContent = editing ? 'Save changes' : 'Add item';
  submitButton.addEventListener('click', async () => {
    const itemId = parseIdFromPickerText(itemInput.value);
    if (isNaN(itemId)) {
      showToast('Enter or pick a valid item ID.', true);
      return;
    }

    const crystalNameId = parseIdFromPickerText(nameInput.value) || 0;

    const payload = {
      category: category,
      itemId: itemId,
      crystalNameId: crystalNameId,
      rank: parseInt(rankField.input.value, 10) || 0,
      element: parseInt(elementField.input.value, 10) || 0,
      quantity: parseInt(quantityField.input.value, 10) || 0,
      buff1Id: parseInt(buffFields[0].id.input.value, 10) || 0,
      buff1Value: parseInt(buffFields[0].value.input.value, 10) || 0,
      buff2Id: parseInt(buffFields[1].id.input.value, 10) || 0,
      buff2Value: parseInt(buffFields[1].value.input.value, 10) || 0,
      buff3Id: parseInt(buffFields[2].id.input.value, 10) || 0,
      buff3Value: parseInt(buffFields[2].value.input.value, 10) || 0,
      buff4Id: parseInt(buffFields[3].id.input.value, 10) || 0,
      buff4Value: parseInt(buffFields[3].value.input.value, 10) || 0,
    };

    if (editing) {
      payload.slotIndex = editing.slotIndex;
    } else if (slotInput.value !== '') {
      payload.slotIndex = parseInt(slotInput.value, 10);
    }

    const response = await callBackend('addOrUpdateGem', payload);
    if (response.success) {
      gemEditState[category] = null;
    }
    applyResponse(response);
  });
  actions.appendChild(submitButton);

  if (editing) {
    const cancelButton = document.createElement('button');
    cancelButton.className = 'btn btn-ghost';
    cancelButton.textContent = 'Cancel';
    cancelButton.addEventListener('click', () => {
      gemEditState[category] = null;
      renderGemForm(category);
    });
    actions.appendChild(cancelButton);
  }

  form.appendChild(actions);
  container.appendChild(form);
}

/* ---------------------------------------------------------------------- */
/* Other items editor (collectables, materials, key items, arts manuals)  */
/* ---------------------------------------------------------------------- */

let itemEditState = { collectable: null, material: null, keyItem: null, artsManual: null };

function renderItemEditor(category) {
  const container = document.getElementById('itemEditor');
  container.innerHTML = '';

  const boxKey = ITEM_BOX_KEYS[category];
  const entries = state.snapshot[boxKey];

  const table = document.createElement('table');
  table.className = 'slot-table';
  table.innerHTML = '<thead><tr><th>Slot</th><th>Item</th><th>Qty</th><th></th></tr></thead>';
  const tbody = document.createElement('tbody');

  if (entries.length === 0) {
    const row = document.createElement('tr');
    row.innerHTML = '<td colspan="4" class="empty-row">No items in this box.</td>';
    tbody.appendChild(row);
  }

  for (const entry of entries) {
    const row = document.createElement('tr');
    const name = getItemName(category, entry.itemId);
    const unnamed = isUnnamedName(name);

    row.innerHTML =
      '<td class="slot-id">' + entry.slotIndex + '</td>' +
      '<td><span class="item-name' + (unnamed ? ' unnamed' : '') + '">' + escapeHtml(name) + '</span>' +
      '<span class="slot-id"> (#' + entry.itemId + ')</span>' +
      (unnamed ? '<span class="badge badge-debug">no game name</span>' : '') + '</td>' +
      '<td></td>' +
      '<td></td>';

    const qtyCell = row.children[2];
    const qtyInput = document.createElement('input');
    qtyInput.className = 'input input-small';
    qtyInput.type = 'number';
    qtyInput.min = '0';
    qtyInput.max = '65535';
    qtyInput.value = String(entry.quantity);
    qtyCell.appendChild(qtyInput);

    const actionsCell = row.lastElementChild;

    const updateQtyButton = document.createElement('button');
    updateQtyButton.className = 'btn btn-small';
    updateQtyButton.textContent = 'Set qty';
    updateQtyButton.style.marginRight = '6px';
    updateQtyButton.addEventListener('click', async () => {
      const response = await callBackend('setItemQuantity', {
        category: category,
        slotIndex: entry.slotIndex,
        quantity: parseInt(qtyInput.value, 10) || 0,
      });
      applyResponse(response);
    });
    actionsCell.appendChild(updateQtyButton);

    const editButton = document.createElement('button');
    editButton.className = 'btn btn-small';
    editButton.textContent = 'Edit';
    editButton.style.marginRight = '6px';
    editButton.addEventListener('click', () => {
      itemEditState[category] = entry;
      renderItemForm(category);
    });
    actionsCell.appendChild(editButton);

    const removeButton = document.createElement('button');
    removeButton.className = 'btn btn-small btn-danger';
    removeButton.textContent = 'Remove';
    removeButton.addEventListener('click', async () => {
      const response = await callBackend('removeItem', { category: category, slotIndex: entry.slotIndex });
      applyResponse(response);
    });
    actionsCell.appendChild(removeButton);

    tbody.appendChild(row);
  }

  table.appendChild(tbody);
  container.appendChild(table);

  renderItemForm(category);
}

function renderItemForm(category) {
  const container = document.getElementById('itemEditor');
  const existingForm = container.querySelector('.editor-form');
  if (existingForm) {
    existingForm.remove();
  }

  const editing = itemEditState[category];

  const form = document.createElement('div');
  form.className = 'editor-form';

  const title = document.createElement('p');
  title.className = 'editor-form-title';
  title.textContent = editing ? ('Editing slot ' + editing.slotIndex) : ('Add a new ' + CATEGORY_LABELS[category]);
  form.appendChild(title);

  const grid = document.createElement('div');
  grid.className = 'form-grid';

  const slotField = document.createElement('div');
  slotField.className = 'form-field';
  slotField.innerHTML = '<label>Slot index (0-499, blank = first free)</label>';
  const slotInput = document.createElement('input');
  slotInput.className = 'input';
  slotInput.type = 'number';
  slotInput.min = '0';
  slotInput.max = '499';
  slotInput.id = 'itemSlotIndex';
  if (editing) {
    slotInput.value = String(editing.slotIndex);
    slotInput.disabled = true;
  }
  slotField.appendChild(slotInput);
  grid.appendChild(slotField);

  const itemField = document.createElement('div');
  itemField.className = 'form-field';
  itemField.innerHTML = '<label>Item</label>';
  const itemInput = document.createElement('input');
  itemInput.className = 'input';
  itemInput.id = 'itemItemId';
  itemInput.setAttribute('list', 'datalist-' + category);
  if (editing) {
    setPickerValueById(itemInput, category, editing.itemId);
  }
  itemField.appendChild(itemInput);
  const datalist = document.createElement('datalist');
  datalist.id = 'datalist-' + category;
  populateDatalist(datalist, category);
  itemField.appendChild(datalist);
  grid.appendChild(itemField);

  const quantityField = buildNumberField('Quantity', 'itemQuantity', editing ? editing.quantity : 1, 0, 65535);
  grid.appendChild(quantityField.wrap);

  form.appendChild(grid);

  const actions = document.createElement('div');
  actions.className = 'form-actions';

  const submitButton = document.createElement('button');
  submitButton.className = 'btn btn-primary';
  submitButton.textContent = editing ? 'Save changes' : 'Add item';
  submitButton.addEventListener('click', async () => {
    const itemId = parseIdFromPickerText(itemInput.value);
    if (isNaN(itemId)) {
      showToast('Enter or pick a valid item ID.', true);
      return;
    }

    const payload = {
      category: category,
      itemId: itemId,
      quantity: parseInt(quantityField.input.value, 10) || 0,
    };

    if (editing) {
      payload.slotIndex = editing.slotIndex;
    } else if (slotInput.value !== '') {
      payload.slotIndex = parseInt(slotInput.value, 10);
    }

    const response = await callBackend('addOrUpdateItem', payload);
    if (response.success) {
      itemEditState[category] = null;
    }
    applyResponse(response);
  });
  actions.appendChild(submitButton);

  if (editing) {
    const cancelButton = document.createElement('button');
    cancelButton.className = 'btn btn-ghost';
    cancelButton.textContent = 'Cancel';
    cancelButton.addEventListener('click', () => {
      itemEditState[category] = null;
      renderItemForm(category);
    });
    actions.appendChild(cancelButton);
  }

  form.appendChild(actions);
  container.appendChild(form);
}

/* ---------------------------------------------------------------------- */
/* Money panel                                                            */
/* ---------------------------------------------------------------------- */

function renderMoneyPanel() {
  document.getElementById('moneyInput').value = String(state.snapshot.money);
  document.getElementById('noponstonesInput').value = String(state.snapshot.noponstones);
}

/* ---------------------------------------------------------------------- */
/* Shared rendering                                                       */
/* ---------------------------------------------------------------------- */

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

function renderAll() {
  renderTopbar();

  if (!state.loaded || !state.snapshot) {
    return;
  }

  renderPartyRoster();
  renderCharacterStatsForm();
  renderEquipEditor('weaponsEditor', 'weapon');
  renderEquipEditor('armorEditor', state.armorCategory);
  renderGemEditor(state.gemCategory);
  renderItemEditor(state.itemCategory);
  renderMoneyPanel();
}

function applyResponse(response) {
  if (!response.success) {
    showToast(response.message || 'Something went wrong.', true);
    return;
  }

  if (response.message) {
    showToast(response.message, false);
  }

  if (response.filePath) {
    state.filePath = response.filePath;
    state.loaded = true;
    state.backupCreated = true;
  }

  if (response.data) {
    state.snapshot = response.data;
    state.loaded = true;
  }

  renderAll();
}

/* ---------------------------------------------------------------------- */
/* Wiring                                                                 */
/* ---------------------------------------------------------------------- */

function wireNavigation() {
  const navItems = document.querySelectorAll('.nav-item');
  navItems.forEach((item) => {
    item.addEventListener('click', () => {
      navItems.forEach((other) => other.classList.remove('active'));
      item.classList.add('active');

      document.querySelectorAll('.panel').forEach((panel) => panel.classList.remove('active'));
      document.getElementById(item.dataset.panel).classList.add('active');
    });
  });
}

function wireTabs(rowId, dataAttribute, onSelect) {
  const row = document.getElementById(rowId);
  row.querySelectorAll('.tab').forEach((tab) => {
    tab.addEventListener('click', () => {
      row.querySelectorAll('.tab').forEach((other) => other.classList.remove('active'));
      tab.classList.add('active');
      onSelect(tab.dataset[dataAttribute]);
    });
  });
}

async function openFile() {
  const response = await callBackend('pickAndLoadFile', {});
  applyResponse(response);
}

function initialize() {
  wireNavigation();

  wireTabs('armorTabs', 'armor', (value) => {
    state.armorCategory = value;
    renderEquipEditor('armorEditor', state.armorCategory);
  });

  wireTabs('gemTabs', 'gem', (value) => {
    state.gemCategory = value;
    renderGemEditor(state.gemCategory);
  });

  wireTabs('itemTabs', 'itemcat', (value) => {
    state.itemCategory = value;
    renderItemEditor(state.itemCategory);
  });

  document.getElementById('btnOpen').addEventListener('click', openFile);
  document.getElementById('btnOpenEmpty').addEventListener('click', openFile);

  document.getElementById('btnSave').addEventListener('click', async () => {
    const response = await callBackend('saveFile', {});
    applyResponse(response);
  });

  document.getElementById('btnSaveAs').addEventListener('click', async () => {
    const response = await callBackend('pickSaveAsLocation', {});
    applyResponse(response);
  });

  document.getElementById('btnAddCharacter').addEventListener('click', async () => {
    const select = document.getElementById('addCharacterSelect');
    const characterId = parseInt(select.value, 10);
    const response = await callBackend('addCharacter', { characterId: characterId });
    applyResponse(response);
  });

  document.getElementById('statsCharacterSelect').addEventListener('change', (event) => {
    state.statsCharacterId = parseInt(event.target.value, 10);
    renderCharacterStatsForm();
  });

  document.getElementById('btnSetMoney').addEventListener('click', async () => {
    const amount = parseInt(document.getElementById('moneyInput').value, 10) || 0;
    const response = await callBackend('setMoney', { amount: amount });
    applyResponse(response);
  });

  document.getElementById('btnSetNoponstones').addEventListener('click', async () => {
    const amount = parseInt(document.getElementById('noponstonesInput').value, 10) || 0;
    const response = await callBackend('setNoponstones', { amount: amount });
    applyResponse(response);
  });

  const artsDatalist = document.createElement('datalist');
  artsDatalist.id = 'artsDatalist';
  populateDatalist(artsDatalist, 'arts');
  document.body.appendChild(artsDatalist);

  renderTopbar();
}

document.addEventListener('DOMContentLoaded', initialize);
