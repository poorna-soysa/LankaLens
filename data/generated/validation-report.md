# LankaLens DataBuilder Validation Report

## Dataset Sources

- Department of Census and Statistics, Sri Lanka - Administrative Division Codes (GND List / MRCB-2024) (dcs-gndlist-final-2024-03-19.xlsx)
- Department of Census and Statistics, Sri Lanka - No. of GN Divisions by DS Division and District (dcs-no-of-gn-by-ds-2024-03-19.xlsx)
- Ministry of Home Affairs, Sri Lanka — Home Affairs Division (IT Unit) - LIFe Location Codes (Grama Niladhari Division List) (moha-life/manifest.json)

## Counts

- Provinces: 9
- Districts: 25
- Divisional Secretariats: 340
- Grama Niladhari Divisions: 14008

## Translation Completeness

- Missing English names: 0
  - Province: 0
  - District: 0
  - DS: 0
  - GN: 0
- Missing Sinhala names: 285
  - Province: 0
  - District: 0
  - DS: 0
  - GN: 285
- Missing Tamil names: 285
  - Province: 0
  - District: 0
  - DS: 0
  - GN: 285

## Issues Summary

- Duplicate codes: 0
- Duplicate names: 47
- Orphans: 0
- Source conflicts: 0
- Warnings: 52
- Errors: 0

## Overall status: PASS

## Issues

- **Warning** `COUNT_DS_MISSING` DivisionalSecretariat/Addalaichchenai: Official counts reference DS 'Addalaichchenai' in district 'Ampara' which was not found.
- **Warning** `COUNT_DS_MISSING` DivisionalSecretariat/Kothmale (East): Official counts reference DS 'Kothmale (East)' in district 'Nuwara Eliya' which was not found.
- **Warning** `COUNT_DS_MISSING` DivisionalSecretariat/Kothmale (West): Official counts reference DS 'Kothmale (West)' in district 'Nuwara Eliya' which was not found.
- **Warning** `COUNT_DS_MISSING` DivisionalSecretariat/Seethawaka (Hanwella): Official counts reference DS 'Seethawaka (Hanwella)' in district 'Colombo' which was not found.
- **Warning** `COUNT_DS_MISSING` DivisionalSecretariat/Waduramba: Official counts reference DS 'Waduramba' in district 'Galle' which was not found.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1206210,1206215: Duplicate Sinhala name 'කුරණ කටුනායක' under parent '1206'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1209125,1209415: Duplicate English GN name 'Halpe' under DS '1209'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1209125,1209415: Duplicate Sinhala name 'හල්පෙ' under parent '1209'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1209125,1209415: Duplicate Tamil name 'ஹல்‍பே' under parent '1209'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1215505,1215520: Duplicate Sinhala name 'අස්ගිරිය දකුණ' under parent '1215'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1227315,1227590: Duplicate English GN name 'Sapugasthenna' under DS '1227'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1227315,1227590: Duplicate Sinhala name 'සපුගස්තැන්න' under parent '1227'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1227535,1227545: Duplicate Tamil name 'உடுகொட' under parent '1227'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1306035,1306060: Duplicate Sinhala name 'වල්ගම දකුණ' under parent '1306'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1321110,1321190: Duplicate English GN name 'Pothupitiya North' under DS '1321'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1339100,1339265: Duplicate English GN name 'Miriswatta' under DS '1339'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1339100,1339265: Duplicate Sinhala name 'මිරිස්වත්ත' under parent '1339'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1339100,1339265: Duplicate Tamil name 'மிரிஸ்வத்த' under parent '1339'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1339230,1339260: Duplicate English GN name 'Gorakadoowa' under DS '1339'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1339230,1339260: Duplicate Sinhala name 'ගොරකදූව' under parent '1339'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/1339230,1339260: Duplicate Tamil name 'கொரகாதூவ' under parent '1339'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/2130125,2130290: Duplicate Tamil name 'லேவெல்ல' under parent '2130'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/2306080,2306410: Duplicate Tamil name 'கங்கஉடகம' under parent '2306'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/3112235,3112255: Duplicate Tamil name 'கஹதூவ' under parent '3112'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/3115155,3115160,3115165: Duplicate Tamil name 'ஹொரன்கல்ல' under parent '3115'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/3135110,3135120: Duplicate Tamil name 'அகுரல வடக்கு' under parent '3135'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/3209165,3209200: Duplicate Sinhala name 'පනාකඩුව බටහිර' under parent '3209'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/4104005,4104015: Duplicate Tamil name 'காரைநகர் வடக்கு' under parent '4104'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/4133185,4133195: Duplicate Tamil name 'கொக்குவில் தென் மேற்கு' under parent '4133'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/5218175,5218180: Duplicate Tamil name 'சென்னல்கிராமம் 01' under parent '5218'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/5224005,5224010: Duplicate English GN name 'Periyaneelavanai Muslim Section' under DS '5224'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/5327155,5327185: Duplicate English GN name 'Jinna Nagar' under DS '5327'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/5327155,5327185: Duplicate Tamil name 'ஜின்னா நகர்' under parent '5327'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6124025,6124215: Duplicate English GN name 'Konwewa' under DS '6124'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6124025,6124215: Duplicate Sinhala name 'කෝන්වැව' under parent '6124'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6124025,6124215: Duplicate Tamil name 'கொன்வெவ' under parent '6124'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6124185,6124210,6124225: Duplicate English GN name 'Kumbukwewa' under DS '6124'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6124185,6124210,6124225: Duplicate Sinhala name 'කුඹුක්වැව' under parent '6124'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6124185,6124210,6124225: Duplicate Tamil name 'கும்புக்வெவ' under parent '6124'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6175075,6175080: Duplicate Tamil name 'சியம்பலாகஸ்ருப்ப மேற்கு' under parent '6175'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6184140,6184155: Duplicate Tamil name 'ஹெந்துவாவ குடியிருப்பு' under parent '6184'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6218130,6218135: Duplicate English GN name 'Udappuwa' under DS '6218'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6218130,6218135: Duplicate Sinhala name 'උඩප්පුව' under parent '6218'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6218130,6218135: Duplicate Tamil name 'உடப்பு' under parent '6218'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6233090,6233095: Duplicate English GN name 'Aluthwatta' under DS '6233'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6233090,6233095: Duplicate Sinhala name 'අළුත්වත්ත' under parent '6233'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/6233090,6233095: Duplicate Tamil name 'அலுத்வத்த' under parent '6233'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/7115075,7115105: Duplicate Tamil name 'மஹா மான்கடவெல' under parent '7115'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/7127100,7127130: Duplicate English GN name 'Hurulunikawewa' under DS '7127'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/7151020,7151120: Duplicate English GN name 'Perimiyankulama' under DS '7151'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/8139060,8139080: Duplicate Tamil name 'பஹல கதுருகமுவ' under parent '8139'.
- **Warning** `DUPLICATE_NAME` GramaNiladhariDivision/9106010,9106015: Duplicate Tamil name 'பத்பேரிய மேற்கு' under parent '9106'.
