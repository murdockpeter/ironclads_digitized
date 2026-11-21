"""Build the two original, shareable Stonewall at the Gates player packets.

Requires reportlab. Run from the repository root; fonts use Windows Calibri/Georgia.
The modern staff formats, orders, signals and scenario rules are original game design.
"""
from pathlib import Path
from xml.sax.saxutils import escape
import math

from reportlab.pdfgen import canvas
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, PageBreak, Table, TableStyle, Flowable,
    KeepTogether,
)
from reportlab.lib import colors
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.enums import TA_LEFT

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Documentation' / 'Briefings'
OUT.mkdir(parents=True, exist_ok=True)
for name, filename in [('Body', 'calibri.ttf'), ('Bold', 'calibrib.ttf'),
                       ('Italic', 'calibrii.ttf'), ('Display', 'georgia.ttf')]:
    pdfmetrics.registerFont(TTFont(name, str(Path('C:/Windows/Fonts') / filename)))
pdfmetrics.registerFontFamily('Body', normal='Body', bold='Bold', italic='Italic', boldItalic='Bold')
NAVY = colors.HexColor('#132B38')
INK = colors.HexColor('#233641')
MUTED = colors.HexColor('#53656B')
PAPER = colors.HexColor('#FAF7F0')
GOLD = colors.HexColor('#B28A4C')
W = 524

SOURCES = [
    ('H1', 'NHHC / DANFS: Stonewall',
     'https://www.history.navy.mil/research/histories/ship-histories/confederate_ships/stonewall.html',
     'Actual European departure and Atlantic arrival; seagoing ram and historical battery.'),
    ('H2', 'NHHC: Trent\u2019s Reach, 23\u201324 January 1865',
     'https://www.history.navy.mil/browse-by-topic/wars-conflicts-and-operations/civil-war/cw-operations-and-engagements/1865-civil-war/trents-reach.html',
     'The failed river sortie and the obstacles to a Confederate passage.'),
    ('H3', 'NHHC / DANFS: Onondaga',
     'https://www.history.navy.mil/research/histories/ship-histories/danfs/o/onondaga-i.html',
     'James River service; two turrets and mixed heavy battery.'),
    ('H4', 'NHHC / DANFS: Mahopac',
     'https://www.history.navy.mil/research/histories/ship-histories/danfs/m/mahopac-i.html',
     'March return to Chesapeake Bay; single turret with two XV-inch guns.'),
    ('H5', 'NHHC / DANFS: Sangamon',
     'https://www.history.navy.mil/research/histories/ship-histories/danfs/s/sangamon-i.html',
     'James River duties and the XV-inch / XI-inch battery.'),
    ('H6', 'NPS: Fort Monroe foundation document',
     'https://www.nps.gov/fomr/learn/management/upload/FOMR_FD_2015_508.pdf',
     'Union stronghold, blockade support and refuge for people escaping slavery.'),
    ('H7', 'U.S. Coast Pilot 3: Chesapeake Bay / Hampton Roads',
     'https://www.nauticalcharts.noaa.gov/publications/coast-pilot/files/cp3/CPB3_WEB.pdf',
     'Modern geographic distinction between Hampton Roads, the lower bay and the Virginia Capes; not 1865 soundings.'),
    ('H8', 'Library of Congress: Under Six Flags',
     'https://blogs.loc.gov/international-collections/2019/06/under-six-flags-the-curious-career-of-the-css-stonewall/',
     'Independent corroboration of Stonewall\u2019s actual chronology.'),
]


class SeaDiagram(Flowable):
    """An original schematic, deliberately not a traced historical or modern chart."""
    def __init__(self, accent):
        Flowable.__init__(self)
        self.width, self.height, self.accent = W, 230, accent

    def draw(self):
        c = self.canv
        c.setFillColor(NAVY)
        c.roundRect(0, 0, W, self.height, 8, fill=1, stroke=0)
        c.setStrokeColor(colors.HexColor('#34545E'))
        c.setLineWidth(.35)
        for x in range(24, 520, 24):
            c.line(x, 16, x, 214)
        for y in range(22, 220, 24):
            c.line(16, y, 508, y)
        c.setFillColor(colors.HexColor('#657166'))
        path = c.beginPath()
        path.moveTo(18, 212)
        for x, y in [(18,165),(95,175),(155,173),(205,186),(239,157),(261,150),(270,174),(286,212)]:
            path.lineTo(x,y)
        path.close()
        c.drawPath(path, fill=1, stroke=0)
        c.setFillColor(colors.HexColor('#7A8170'))
        c.circle(266, 92, 9, fill=1, stroke=0)
        c.setFillColor(colors.white)
        c.setFont('Bold', 9)
        c.drawString(28,192,'LOWER JAMES / HAMPTON ROADS')
        c.drawString(181,198,'OLD POINT COMFORT')
        c.setFillColor(GOLD)
        c.rect(254,153,9,9,fill=1,stroke=0)
        c.drawString(202,139,'FORT MONROE')
        c.rect(262,88,8,8,fill=1,stroke=0)
        c.drawString(229,71,'FORT WOOL')
        c.setStrokeColor(self.accent)
        c.setLineWidth(2.8)
        points=[(56,113),(157,116),(233,123),(313,126),(390,126),(473,126)]
        p=c.beginPath();p.moveTo(*points[0])
        for point in points[1:]:p.lineTo(*point)
        c.drawPath(p)
        c.line(473,126,462,132);c.line(473,126,462,120)
        c.setStrokeColor(GOLD);c.setDash(4,3)
        c.line(363,43,363,195);c.setDash()
        c.setFillColor(colors.white);c.setFont('Bold',9)
        c.drawString(372,181,'GATE LIBERTY')
        c.setFont('Body',8)
        c.drawString(372,168,'column 184 / rows 22\u201370')
        c.drawString(30,94,'Opening contact area')
        c.drawString(30,81,'Ships maneuver eastward')
        c.drawString(385,106,'Lower Chesapeake')
        c.drawString(385,93,'Capes / Atlantic onward')
        c.setFillColor(GOLD);c.setFont('Bold',8)
        c.drawString(22,21,'ORIGINAL SCHEMATIC \u2022 NOT TO SCALE \u2022 NO DEPTH OR FIRE-ARC INFORMATION')


class Packet:
    def __init__(self, side, accent):
        self.side, self.accent, self.story = side, colors.HexColor(accent), []
        self.styles = {
            'body': ParagraphStyle('Body',fontName='Body',fontSize=10.2,leading=13.5,
                                   textColor=INK,spaceAfter=6),
            'small': ParagraphStyle('Small',fontName='Body',fontSize=8.4,leading=11.1,
                                    textColor=MUTED,spaceAfter=6),
            'h1': ParagraphStyle('Title',fontName='Display',fontSize=25,leading=29,
                                 textColor=NAVY,spaceAfter=15),
            'h2': ParagraphStyle('Section',fontName='Bold',fontSize=12.2,leading=15,
                                 textColor=colors.HexColor(accent),spaceBefore=8,spaceAfter=4,
                                 keepWithNext=True),
            'cell': ParagraphStyle('Cell',fontName='Body',fontSize=8.6,leading=11.1,
                                   textColor=INK),
            'cellhead': ParagraphStyle('CellHead',fontName='Bold',fontSize=8.4,leading=10.5,
                                       textColor=colors.white),
        }

    def p(self,text,style='body'):
        self.story.append(Paragraph(text,self.styles[style]))

    def h(self,text):
        self.p(text,'h2')

    def page(self,kicker,title):
        self.story.append(PageBreak())
        self.p(kicker.upper(),'small')
        self.p(title,'h1')

    def box(self,label,text):
        t=Table([[Paragraph(label.upper(),self.styles['h2'])],
                 [Paragraph(text,self.styles['body'])]],colWidths=[W])
        t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),colors.HexColor('#ECE8DD')),
                               ('BOX',(0,0),(-1,-1),.6,self.accent),
                               ('LEFTPADDING',(0,0),(-1,-1),12),
                               ('RIGHTPADDING',(0,0),(-1,-1),12),
                               ('TOPPADDING',(0,0),(-1,-1),7),
                               ('BOTTOMPADDING',(0,0),(-1,-1),7)]))
        self.story += [t,Spacer(1,9)]

    def table(self, headers, rows, widths):
        cells=[[Paragraph(escape(x),self.styles['cellhead']) for x in headers]]
        cells += [[Paragraph(x,self.styles['cell']) for x in row] for row in rows]
        t=Table(cells,colWidths=widths,repeatRows=1,hAlign='LEFT')
        t.setStyle(TableStyle([
            ('BACKGROUND',(0,0),(-1,0),NAVY),('VALIGN',(0,0),(-1,-1),'TOP'),
            ('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.HexColor('#EFECE3'),colors.HexColor('#FAF7F0')]),
            ('LINEBELOW',(0,0),(-1,0),1,self.accent),
            ('LINEBELOW',(0,1),(-1,-1),.35,colors.HexColor('#D3D4CC')),
            ('LEFTPADDING',(0,0),(-1,-1),8),('RIGHTPADDING',(0,0),(-1,-1),8),
            ('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7),
        ]))
        self.story += [t,Spacer(1,8)]

    def decoration(self,c,doc):
        c.saveState()
        if doc.page==1:
            c.setFillColor(NAVY);c.rect(0,0,612,792,fill=1,stroke=0)
            c.setStrokeColor(colors.HexColor('#36505A'));c.setLineWidth(.5)
            for y in range(-100,850,38):
                c.line(330,y,612,y+160)
            c.setFillColor(self.accent);c.rect(44,715,78,5,fill=1,stroke=0)
            c.setFillColor(colors.HexColor('#B9C8CD'));c.setFont('Body',9)
            c.drawString(44,35,'PLAYER EDITION 01  /  21 NOVEMBER 2026  /  ALTERNATE HISTORY')
        else:
            c.setFillColor(PAPER);c.rect(0,0,612,792,fill=1,stroke=0)
            c.setFillColor(self.accent);c.rect(0,750,612,42,fill=1,stroke=0)
            c.setFillColor(colors.white);c.setFont('Bold',8.6)
            c.drawString(44,767,'STONEWALL AT THE GATES')
            c.drawRightString(568,767,self.side.upper()+'  /  PLAYER BRIEFING')
            c.setStrokeColor(colors.HexColor('#D1D0C8'));c.line(44,43,568,43)
            c.setFillColor(MUTED);c.setFont('Body',8)
            c.drawString(44,28,'28 MARCH 1865  /  FICTIONAL ORDERS \u2022 SHARED MATCH RULES ON PAGE 3')
            c.drawRightString(568,28,f'{doc.page:02d}')
        c.restoreState()

    def cover(self,subtitle,mission,who,why):
        white=ParagraphStyle('Cover',fontName='Display',fontSize=38,leading=44,textColor=colors.white,spaceAfter=22)
        pale=ParagraphStyle('CoverBody',fontName='Body',fontSize=12,leading=17,textColor=colors.HexColor('#D6E0E2'),spaceAfter=17)
        label=ParagraphStyle('CoverLabel',fontName='Bold',fontSize=11,leading=15,textColor=self.accent,spaceAfter=15)
        self.story.append(Spacer(1,25))
        self.story.append(Paragraph(self.side.upper()+' COMMAND PACKET',label))
        self.story.append(Paragraph('Stonewall<br/>at the Gates',white))
        self.story.append(Paragraph(subtitle,pale))
        self.story.append(Spacer(1,18))
        self.story.append(Paragraph(mission,ParagraphStyle('Mission',parent=pale,fontName='Bold',fontSize=16,leading=22)))
        self.story.append(Spacer(1,18))
        for title,text in [('WHO',who),('WHEN / WHERE','28 March 1865, 14:00\u201317:00 UTC in the game. Hampton Roads, the Fort Monroe / Fort Wool passage, and the lower Chesapeake exit.'),('WHY',why)]:
            self.story.append(Paragraph(title,label))
            self.story.append(Paragraph(text,pale))
        self.story.append(Spacer(1,9))
        self.story.append(Paragraph('METT-TC  /  OCOKA  /  FIVE-PARAGRAPH SMEAC  /  SYNCHRONIZATION MATRIX',
                                    ParagraphStyle('Contents',parent=pale,fontSize=9,leading=13)))

    def build(self):
        dest=OUT/f'Stonewall-at-the-Gates-{self.side}.pdf'
        doc=SimpleDocTemplate(str(dest),pagesize=(612,792),leftMargin=44,rightMargin=44,
                              topMargin=66,bottomMargin=58,
                              title=f'Stonewall at the Gates | {self.side} Player Briefing',
                              author='Ironclads Digitized | Scenario Design',
                              subject='Alternate-history naval wargame: METT-TC, OCOKA, SMEAC and synchronization')
        doc.build(self.story,onFirstPage=self.decoration,onLaterPages=self.decoration)
        print(dest)


def common_history(p):
    p.page('01 / Historical framing','A plausible mission.\nAn explicit divergence.')
    p.h('What the historical record supports')
    p.p('A Confederate attempt to get ironclads out of the James is a credible operational problem. The January 1865 Trent\u2019s Reach sortie failed; passage through obstructions, shoals and Union defenses was a serious obstacle, not an administrative movement. [H2]')
    p.p('Stonewall was still departing Ferrol on 24 March and reached Nassau on 6 May. She therefore could not have fought here on 28 March in the real chronology. [H1, H8] Onondaga and Sangamon had James River service; Mahopac returned to Chesapeake Bay in March. Their concentration here is a scenario assignment, not a documented battle formation. [H3\u2013H5]')
    p.h('What this game deliberately changes')
    p.p('<b>Early arrival.</b> Repairs and logistics are completed early enough for Stonewall to reach Virginia by mid-March. Under invented orders, she enters the lower James to cover a squadron withdrawal, rather than remaining an independent raider.')
    p.p('<b>Successful river passage.</b> Virginia II and Fredericksburg clear the upper-river defenses during the night of 27\u201328 March. A prearranged lighter provides the last coal and stores before the three ships assemble. Neither passage nor replenishment is claimed as fact; neither is replayed on this table.')
    p.p('<b>Delayed collapse.</b> For the wider campaign, assume the Union spring offensive is delayed or checked and organized fighting continues into summer. This is an author-level counterfactual, not intelligence that captains can know on 28 March. The local escape mission also makes sense without an extended war.')
    p.h('Why the objective works')
    p.p('The Confederate purpose is force preservation and a continuing coastal threat, not a miraculous reversal of the land war. Stonewall is the strongest narrative candidate for subsequent ocean operations; a surviving river ironclad may instead disperse along a sheltered coast. Ocean transit in either river vessel requires favorable conditions and is a further game assumption. Any of the three still satisfies the agreed tactical objective.')
    p.box('Geography matters','Fort Monroe guards access between Hampton Roads and the lower Chesapeake; it is not the Atlantic shoreline. The Virginia Capes lie farther east. Gate LIBERTY is an explicit tabletop abstraction of a successful onward passage, not a claim that the open ocean begins at the fort. [H6, H7]')
    p.p('METT-TC, OCOKA, SMEAC and the matrix are modern analytical formats applied retrospectively. The orders, task organization and signal words in these packets are invented for play; they are not recovered 1865 documents.','small')


def common_contract(p):
    p.page('02 / Identical in both packets','The match contract')
    p.box('Decisive end state','CONFEDERATE VICTORY: at least ONE of Stonewall, Virginia II or Fredericksburg escapes under its own power, free and clear, through Gate LIBERTY beyond the forts. UNION VICTORY: no Confederate ironclad completes that escape by the deadline. Stonewall is not a mandatory survivor.')
    p.h('1. Board, opening and clock')
    p.p('Use the expanded coastal build, 100-yard hexes and three-minute turns. Keep the six displayed starting positions; each side may write initial headings before Turn 1, then reveal them together without a movement charge. All other movement follows the agreed ship cards. Play <b>60 resolved turns</b>, from 14:00 to 17:00 UTC in game time. The old 20-turn / one-hour suggestion is superseded: the enlarged approach needs more transit time. Keep a separate resolved-turn ledger.')
    p.h('2. Gate LIBERTY: mark before play')
    p.p('The gate is the line of hex centers at <b>column 184, rows 22 through 70 inclusive</b>. A candidate must cross from west to east through that segment; entering from its north or south end does not count. These are expanded-board local coordinates, not printed Yaquinto mapboard IDs. The gate is roughly 13 km east of the scenario origin and east of both forts. The umpire marks it on the common chart; the app does not draw or score this gate automatically.')
    p.h('3. What \u201cfree and clear\u201d means')
    p.p('After crossing, record a clear count at the end of each resolved turn. Count a turn only if the candidate is east of the gate, afloat, steerable, moving under its own power, not surrendered or under tow, and with no unresolved flooding or fire that the agreed damage rules make fatal. <b>No operational Union monitor may cover it with functional direct-fire guns</b> at that turn\u2019s resolution: use declared effective range, functional guns, bearing, visibility and line of fire, not the decorative measuring arc. For this clearance check only, treat functional guns as loaded: an enemy reload interval does not make a ship free and clear. Reset the count to zero whenever a condition fails. <b>Two consecutive qualifying turns win.</b>')
    p.p('Resolve all movement, gunfire, ramming and damage for the turn before checking victory. A ship lost in the same turn cannot win. Do not remove a candidate at the board edge before its status is resolved; the umpire records its continuation in an off-map holding box under the same conditions. Under this baseline, onward passage through the Capes is then conceded and no fresh off-map interception occurs.')
    p.h('4. Deadline and common restrictions')
    p.p('A ship on only one clear turn at Turn 60 has not escaped: Union victory. End early if all three Confederate ships are sunk, captured or scuttled. Extra escapees improve the narrative outcome but do not alter the one-ship threshold. No new ships, hidden mines, surprise shore guns or unbriefed civilian traffic may be introduced. No draw or casualty-points override is used.')
    p.p('<b>Forts:</b> Union-held throughout this scenario. Baseline separate shore-battery fire is OFF, an explicit balance abstraction, not a historical claim that the forts were harmless. An active-fort variant needs written battery cards, arcs, rates, ranges and target rules issued to both sides before Turn 1. No such variant is assumed here. Shoals, depths and current are likewise adjudicated only from a common premarked playable-water overlay.','small')


def terrain(p, side):
    p.page('03 / OCOKA','Read the water, not just the enemy')
    p.story += [SeaDiagram(p.accent),Spacer(1,12)]
    p.p('Shared operational picture: lower James \u2192 Hampton Roads \u2192 passage past Monroe and Wool \u2192 Gate LIBERTY \u2192 an abstracted Capes transit. This original schematic is not a shoreline survey. The game\u2019s modern-data terrain does not establish 1865 navigable depths.','small')
    if side=='Confederate':
        items=[
            ('O / Observation & fields of fire','Locate the Union intercepting line before committing the runner. Smoke may conceal intent but also hide signals and the next turn. Turrets and casemates have different bearing problems; verify actual card arcs.'),
            ('C / Cover & concealment','Armor is the principal protection. Shore silhouettes do not confer immunity. Use a sacrificial change of bearing only when it creates measurable eastward progress for a survivor; do not mistake smoke effects for adjudicated concealment.'),
            ('O / Obstacles','Land, declared shoals, disabled ships and enemy rams can close a route. Approach the fort passage with room to turn. No ship may cross land or invent a safe channel because the app permits a placement.'),
            ('K / Key terrain','The passage abreast of the forts is the route bottleneck; Gate LIBERTY is the decision boundary. The route matters more than possession of any shoreline hex. Neither a fort capture nor a monitor kill is required.'),
            ('A / Avenues of approach','Favor the jointly declared central water route. A feint may draw a monitor away, but a north or south end-run must still return to the legal gate segment. Preserve an alternate turn away from a collision trap.'),
        ]
    else:
        items=[
            ('O / Observation & fields of fire','Report the leading escape candidate and its route, not merely the nearest target. Maintain overlapping legal engagement opportunities. Check arcs and loading before firing; for the clearance count, functional covering guns still deny freedom while reloading.'),
            ('C / Cover & concealment','Low hull profiles and armor are advantages, not invulnerability. Funnel and powder smoke can mask the runner. Keep enough lateral spacing to retain sight lines and avoid hiding one monitor behind another.'),
            ('O / Obstacles','Use declared shoals and land to limit enemy options, but do not manufacture obstruction hexes. A disabled monitor in the passage can obstruct friends as well as opponents; announce its status promptly.'),
            ('K / Key terrain','The fort passage and the water immediately west of LIBERTY control escape. Holding an interception lane has greater value than pursuing a damaged opponent westward. No Confederate hull may be ignored solely because Stonewall is engaged.'),
            ('A / Avenues of approach','Expect a main runner with a fixing action, or a split breakout. Cover the common eastward route with a mobile reserve rather than spreading every ship to a map edge. North and south feints still converge on the marked gate.'),
        ]
    for h,t in items:p.h(h);p.p(t)


def mett(p, side):
    p.page('04 / METT-TC','The commander\u2019s estimate')
    if side=='Confederate':
        rows=[
            ('MISSION','Extract one ironclad beyond the Union interception system by 17:00. The supported ship is whichever vessel still offers the best escape prospect, not automatically the flagship. A costly gun duel without an escaping hull is failure.'),
            ('ENEMY','Three monitors can trade time for position and concentrate heavy fire on a selected runner. Most likely: a blocking pair with one reserve. Most dangerous: a deliberate gap that invites a ram or close-range crossfire, followed by a fast shift against the actual survivor.'),
            ('TERRAIN & WEATHER','The forts identify a chokepoint; the gate lies farther east. Use only predeclared navigable water. Baseline weather is the game preset: westerly wind 7 knots, wave height 0.25 m, visibility 12 km, light cloud. These are invented conditions; no historical tide advantage is assumed.'),
            ('TROOPS & SUPPORT','Stonewall supplies the seagoing rationale and a strong forward threat. Virginia II and Fredericksburg provide armor, guns and alternative escape candidates. No tugs, tenders, shore support or relief force is available after H-hour. Printed cards govern actual performance.'),
            ('TIME AVAILABLE','Sixty turns include transit, maneuver and the two-turn clear count. Recalculate distance-to-gate against current permitted movement every five turns and after damage. Do not plan a crossing on Turn 60: a candidate needs time to qualify twice. Phase changes follow events, not an inflexible schedule.'),
            ('CIVIL CONSIDERATIONS','The roadstead contains ports, shore communities and people outside the fight. No civilian targets or prize ships are deployed; firing on them brings no score. Fort Monroe\u2019s historical refuge role is part of the setting, not an objective to attack. Preserve boat crews when tactical conditions allow. [H6]'),
        ]
        conclusion='Choose a runner, buy its freedom with the other hulls, and preserve a second candidate if the first loses speed. The limiting resource is time under interception, not the number of cannon effects you can trigger.'
    else:
        rows=[
            ('MISSION','Deny escape to every Confederate ironclad through Turn 60. Keep the harbor defense coherent and at least one ship positioned to influence LIBERTY. Destruction is one means; delay and forced retreat also achieve the mission.'),
            ('ENEMY','Stonewall, Virginia II and Fredericksburg need only one survivor to escape. Most likely: one runner, two ships fixing your line. Most dangerous: a split approach that forces premature reserve commitment, then a change of runner after you have turned away.'),
            ('TERRAIN & WEATHER','The route past Monroe and Wool restricts maneuver; the gate lies beyond it. Use the shared water overlay, not visual coast colors, for navigation. Westerly wind 7 knots, waves 0.25 m and visibility 12 km are scenario presets. No unbriefed favorable current or shore fire may rescue a broken screen.'),
            ('TROOPS & SUPPORT','Onondaga has two independently trained turrets; Mahopac and Sangamon each have one. Their practical movement, armor, gun effects and reloads come from the cards. No additional monitors arrive. The baseline forts are Union-held scenery without separately rolled fire.'),
            ('TIME AVAILABLE','The deadline is your ally only while the route remains covered. Each enemy turn spent maneuvering west of the gate advances denial. Plan interception and reload cycles before the candidate crosses; two consecutive clear turns end the match after full resolution.'),
            ('CIVIL CONSIDERATIONS','Protect the harbor function and avoid firing through friendly ships or declared noncombatant areas. No civilian targets are deployed. Treat rescued crews and surrendered vessels as removed from combat. Historical Fort Monroe served military operations and refugees from slavery; neither is a disposable terrain marker. [H6]'),
        ]
        conclusion='Defend a moving interception problem, not three personal duels. Keep target assignments explicit, hold a reserve while possible, and switch effort to the hull closest to satisfying the escape conditions.'
    for h,t in rows:p.h(h);p.p(t)
    p.box('Assessment',conclusion)


def situation_mission(p,side):
    p.page('05 / SMEAC \u2022 paragraphs 1\u20132','Situation & mission')
    p.p('The following is a fictional five-paragraph order, written for this match. Ship-command appointments are player roles; no historical officer is asserted to have issued it.','small')
    p.h('1. SITUATION')
    if side=='Confederate':
        p.p('<b>a. General.</b> The squadron has reached the lower James after a hazardous night passage. Withdrawal upstream risks renewed confinement; the roadstead offers a narrow chance to save an armored force. Richmond remains in Confederate hands in this campaign branch, but no assurance of relief is available. Stonewall\u2019s last replenishment is complete. All ships are committed with stores aboard.')
        p.p('<b>b. Enemy.</b> Onondaga, Mahopac and Sangamon are concentrating to stop the breakout. Assume their heavy guns can disable an isolated runner and their captains understand that one escape defeats them. Their exact first-turn orders are unknown. Fort Monroe and Fort Wool remain hostile positions; separate battery fire is excluded by the common baseline, not by Confederate action.')
        p.p('<b>c. Friendly.</b> Stonewall, Virginia II and Fredericksburg form the breakout group. The senior player initially commands from Stonewall. Virginia II is the initial supporting ship; Fredericksburg is the initial alternate runner / rear guard. Those roles may change by signal. No ship is compelled to remain in a losing duel merely to maintain a formation.')
        p.p('<b>d. Attachments and assumptions.</b> None. No scouts, tug rescue, mines, remote batteries or surprise reinforcements are attached. Onward passage beyond LIBERTY is conceded only after the shared clear-count criteria are met. Enemy positions shown by the app are shared table information unless the umpire has issued a written visibility overlay.')
        mission='The Confederate breakout group, comprising Stonewall, Virginia II and Fredericksburg, will force a passage from Hampton Roads past Fort Monroe and Fort Wool beginning at 14:00 on 28 March 1865, and secure at least one self-propelled ironclad free and clear through Gate LIBERTY by 17:00, in order to preserve an armored threat for continued operations beyond the James.'
        intent='Purpose: prevent the squadron\u2019s confinement or destruction. Method: concentrate maneuver and supporting fire to free one runner, switching runners when needed. End state: one or more qualifying ironclads in the open-sea box; enemy losses are incidental to that result.'
    else:
        p.p('<b>a. General.</b> The Confederate squadron has passed the upper-James defenses and is approaching the roadstead. A seagoing Confederate ram has joined two river ironclads. A successful escape would force renewed searches and escorts and undermine confidence in the blockade. The harbor defense must prevent that operational problem before the enemy clears the lower bay.')
        p.p('<b>b. Enemy.</b> Stonewall provides a forward gun / ram threat; Virginia II and Fredericksburg are alternate escape candidates as well as supporting casemates. Treat reported damage as information, not proof of helplessness. An apparently sacrificial ship may become the runner when your force changes bearing. Expect deception within the shared information rules.')
        p.p('<b>c. Friendly.</b> Onondaga is initially the division command ship and central blocker. Mahopac is initially the close support / contact ship. Sangamon is initially the mobile reserve covering the uncommitted route. These are game task assignments. No historical command relationship or exact 28 March station is claimed. Forts remain Union-held; no independent battery fire is rolled in the baseline.')
        p.p('<b>d. Attachments and assumptions.</b> None. No additional gunboats or seaward interception force will arrive. There is no automatic save from an off-map blockade once a candidate qualifies at LIBERTY. All commanders share the declared navigable-water and line-of-fire rules. Protect the exit as the primary task even if a damaged enemy remains behind you.')
        mission='The Union intercepting division, comprising Onondaga, Mahopac and Sangamon, will prevent every Confederate ironclad from escaping Hampton Roads past Fort Monroe and Fort Wool through Gate LIBERTY from 14:00 until 17:00 on 28 March 1865, in order to preserve control of the roadstead and deny the Confederacy an armored force at sea.'
        intent='Purpose: contain the entire breakout group. Method: a coherent blocking screen, deliberate target handoffs and a reserve able to close the most dangerous lane. End state: no qualifying Confederate escape at the deadline; sinking all opponents is unnecessary.'
    p.h('2. MISSION')
    p.box('Who / what / when / where / why',mission)
    p.p('<b>Commander\u2019s intent.</b> '+intent)


def execution(p,side):
    p.page('06 / SMEAC \u2022 paragraph 3','Execution')
    if side=='Confederate':
        sections=[
            ('a. Concept of operations','Conduct a breakout, not a line-of-battle contest. Initially designate Stonewall RUNNER, Virginia II SHIELD and Fredericksburg RELIEF. Seek a navigable lane with the fewest legal Union engagements. The runner continues toward LIBERTY while supporting ships disrupt the interceptors. Reassign roles immediately if machinery or steering makes another hull the better survivor.'),
            ('b. Phase I \u2014 form and identify','At H-hour, reveal initial headings, confirm cards and gate, then build eastward progress while reading the enemy line. Virginia II fixes the most immediate blocker without masking Stonewall\u2019s forward fire. Fredericksburg retains room to cover a turn or become the runner. Do not spend the opening exchanging fire that does not change the geometry.'),
            ('c. Phase II \u2014 rupture the screen','On OPEN, exploit an actual gap. A threat to ram is useful if it forces a monitor away; executing a ram is justified only if the agreed rules and expected escape gain support it. The supporting ship accepts a fixing task while the runner avoids unnecessary turns. Keep supporting fire clear of the intended route. Do not assume a monitor is neutralized because its turret faces elsewhere.'),
            ('d. Phase III \u2014 pass the forts','On GATE, the runner commits through the declared water passage and toward the marked exit. SHIELD prevents a close interceptor from obtaining a legal engagement; RELIEF checks the next enemy ship or follows through a different navigable lane. Passage past the fort buildings alone is not victory. Maintain the agreed ordering and loading sequence while moving.'),
            ('e. Phase IV \u2014 secure escape','Once east of LIBERTY, continue on an eastward course with sea room; do not stop on the line to trade salvos. Report CLEAR ONE and CLEAR TWO only after umpire confirmation. Union gun coverage resets the count. The runner remains an active target until the second qualifying turn is fully resolved. The other ships need not escape for the primary victory.'),
            ('f. Tasks to individual ships','Stonewall: initially exploit the best lane and preserve mobility. Virginia II: initially fix the closest blocker and protect the runner\u2019s turn. Fredericksburg: initially cover separation, retain the alternate route, and be ready to become the supported ship. All captains report a change in maximum permitted movement before offering tactical advice.'),
            ('g. Coordinating instructions','Change of runner: RELIEF plus ship name, acknowledged by both supports. NO WAY means the intended route is blocked; do not repeat a failed turn blindly. A disabled ship may fight to cover another, surrender, or scuttle only under the agreed rules; it cannot count as escaped. Towing does not satisfy the victory condition. Check transit arithmetic every five turns and prioritize the final two qualifying turns.'),
        ]
    else:
        sections=[
            ('a. Concept of operations','Deny the best escape route with mutually supporting monitors and preserve a reserve against a change of runner. Each captain tracks all three Confederate hulls. Do not assign permanent personal opponents. The division changes effort according to which ship is closest to achieving the two-turn clear count, not which ship has inflicted the most damage.'),
            ('b. Phase I \u2014 establish the screen','At H-hour, reveal headings and confirm sectors. Onondaga holds the principal blocking position with both turrets prepared for useful bearings. Mahopac makes contact without opening a clear lane astern. Sangamon remains able to cover the uncommitted eastward route. Avoid a close formation in which one disabled ship obstructs every friendly turn.'),
            ('c. Phase II \u2014 identify and fix','Report RUNNER plus ship name when enemy movement reveals the main effort. Mahopac pressures that route while Onondaga prepares a legal engagement at the likely crossing. Do not empty every gun into a supporting ship just before the runner becomes exposed. Sangamon shifts only far enough to threaten the gap; an enemy feint should not draw the reserve out of the exit problem.'),
            ('d. Phase III \u2014 close the gate','On CLOSE, the nearest suitable monitor takes the candidate while the next ship covers its evasive turn. Onondaga coordinates turret arcs and target changes; independent turrets do not remove the need for legal loading and bearing checks. A ram is an option under the agreed rules, not an excuse to leave an unopposed lane for another Confederate ironclad.'),
            ('e. Phase IV \u2014 defeat the clear count','If a candidate crosses, call CROSSING and announce whether functional guns cover the candidate under the shared gate rule. Complete all movement and firing before the umpire checks the count. Reestablishing functional gun coverage resets it; firing an impossible or out-of-range shot does not. Keep the other two enemy hulls covered while the best-positioned monitor pursues.'),
            ('f. Tasks to individual ships','Onondaga: initially command the center, allocate targets and retain useful independent turret arcs. Mahopac: initially fix the leading threat and force course changes. Sangamon: initially preserve the reserve / backstop and block the neglected route. All captains keep the division advised of gun readiness, mobility and whether the next turn\u2019s target can actually be engaged.'),
            ('g. Coordinating instructions','Target handoff requires receiver acknowledgement before the previous blocker abandons its lane. HOLE plus location announces an uncovered approach; SWITCH plus ship name redirects effort. If the reserve is disabled, the commander explicitly redraws sectors. Do not pursue west for a trophy while a second candidate advances east. At Turn 60, hold the screen through all final damage and victory checks.'),
        ]
    for h,t in sections:p.h(h);p.p(t)


def admin_command(p,side):
    p.page('07 / SMEAC \u2022 paragraphs 4\u20135','Sustainment, command & signals')
    p.h('4. ADMINISTRATION & LOGISTICS')
    if side=='Confederate':
        p.p('<b>a. Material.</b> Begin with the ship cards\u2019 agreed ammunition, engine state and damage status; no replenishment occurs after H-hour. Coal and boiler endurance sufficient for the match and abstracted onward passage are granted by scenario assumption, not calculated from a historical bunker figure. Protect machinery and steering because an immobile survivor cannot meet the mission.')
        p.p('<b>b. Casualties and repair.</b> Apply the agreed repair, fire and flooding procedures. Report effects in game terms: permitted movement, steering, available battery, repair prospects. Casualty treatment and boat recovery are abstracted; they consume time only where the chosen rules say so. No casualty total substitutes for an escaping ironclad.')
        p.p('<b>c. Disabled vessels.</b> The squadron commander decides whether a disabled ship can still cover another. No salvage tug is coming. Towing may be narrated only if rules permit, and a towed vessel does not count as the qualifying survivor. Surrendered or scuttled vessels are permanently ineligible.')
        chain='Stonewall commander, then Virginia II commander, then Fredericksburg commander. Loss of the flagship alone does not end the match. Announce succession in the shared channel; a change in command ship does not automatically change the runner.'
        priorities='Route opening; maximum current movement; failed steering; enemy blocker positions; legal gun engagements; runner crossing; clear count.'
        words=[('OPEN','Exploit the named navigable gap.'),('RELIEF + ship','Transfer the supported runner role.'),('NO WAY + location','The intended lane is blocked.'),('GATE','Commit to the passage and exit approach.'),('CLEAR ONE / TWO','Umpire-confirmed qualifying turns, not a captain\u2019s claim.')]
    else:
        p.p('<b>a. Material.</b> Start with the ammunition, engine state and damage status fixed by the agreed cards. Do not infer unlimited heavy-gun readiness from a visual fire button. There is no reload bonus for standing near a fort and no ammunition transfer between monitors during the match. Maintain a written battery-readiness record.')
        p.p('<b>b. Casualties and repair.</b> Apply the chosen damage and repair system. Report the loss of mobility or a battery immediately so another ship can cover the sector. Rescue and medical activity are abstracted unless the game rules assign a specific movement or action cost. A ship under repair still has a coverage consequence.')
        p.p('<b>c. Captures and disabled hulls.</b> Mark the exact position and status of surrendered, sunk or immobile vessels. They block movement or sight only as the common rules specify. No prize crew, tug or reinforcement enters automatically. Protect your remaining mobile hulls; a functioning gun aboard a badly placed monitor may be less useful than a reserve with a clear approach.')
        chain='Onondaga commander, then Mahopac commander, then Sangamon commander. On succession, the new commander immediately restates sectors, the current enemy runner and the reserve assignment.'
        priorities='Leading candidate; lane or gate threat; own next legal engagement; gun readiness; mobility loss; target handoff; candidate crossing and clear count.'
        words=[('RUNNER + ship','Identify the most dangerous escape candidate.'),('HOLE + location','An approach or interception sector is uncovered.'),('SWITCH + ship','Reassign effort to the named enemy hull.'),('CLOSE','Execute the coordinated interception.'),('CROSSING','A candidate crossed LIBERTY; defeat the qualifying count.')]
    p.h('5. COMMAND & SIGNAL')
    p.p('<b>a. Command and succession.</b> '+chain)
    p.p('<b>b. Communications.</b> Captains submit orders in the agreed simultaneous sequence. Signal words below are fictional game shorthand, not an authentic Civil War signal book. Within the team, planning chat is allowed between turns. Once orders are locked, changes await the next turn except where the umpire applies a written exception. If using multiplayer, the host assigns ship control; the app does not confer tactical command authority.')
    p.p('<b>c. Essential reports.</b> '+priorities+' Use ship name, resolved-turn number, hex or route location, and requested action. Confirm critical role / target changes with a readback. If communication fails, continue the last acknowledged task in support of commander\u2019s intent.')
    p.table(['Signal','Meaning'],words,[138,386])
    p.p('<b>Common resolution sequence:</b> written orders \u2192 simultaneous reveal \u2192 movement / turns under the chosen rules \u2192 eligible fire and rams \u2192 damage / repair \u2192 gate eligibility and clear count \u2192 advance clock. Record any ruleset-specific timing variation before play; it must be identical for both sides.','small')


def sync(p,side):
    p.page('08 / Synchronization matrix','Coordinate by event, not hope')
    p.p('Planning windows are advisory. The phase trigger governs; commanders may advance, delay or reassign tasks. Turn 1 starts at 14:00; Turn 60 resolves at 17:00. Nothing in this matrix grants extra movement, firing or signals.','small')
    if side=='Confederate':
        heads=['Phase / trigger','Stonewall','Virginia II','Fredericksburg','Command / decision']
        rows=[
            ['I / FORM\nT1\u201310\n14:00\u201314:30','Initial runner. Build eastward progress; identify the usable lane.','Initial shield. Fix the nearest actual blocker without masking the route.','Alternate runner. Preserve room for a change of role.','Name runner and route. Confirm distance / remaining turns. Trigger: a usable gap is identified.'],
            ['II / OPEN\nT11\u201330\n14:30\u201315:30','Exploit the gap; keep mobility ahead of firing opportunities.','Force the interceptor to turn or delay; keep supporting arcs clear.','Cover separation or become runner if RELIEF is signaled.','If the runner loses time or mobility, transfer support promptly. Trigger: committed passage past the forts.'],
            ['III / GATE\nT31\u201350\n15:30\u201316:30','Continue toward LIBERTY if still the best survivor.','Protect the runner\u2019s evasive turn; avoid a blocking friendly collision.','Pressure the next pursuer or exploit a second lane.','Check all three escape prospects. Trigger: a candidate crosses LIBERTY.'],
            ['IV / CLEAR\nT51\u201360\n16:30\u201317:00','Whichever ship is runner continues east and stays eligible.','Support the current runner; do not demand its return to a duel.','Support or become the alternate candidate before time expires.','Umpire checks two consecutive clear turns. Final qualifying sequence must finish by T60.'],
        ]
        p.table(heads,rows,[79,109,109,109,118])
        p.h('Branches / decisions')
        p.p('<b>Runner disabled:</b> RELIEF plus ship name; supports acknowledge before changing course. <b>Passage closed:</b> NO WAY; reassess a declared alternate rather than sail through land. <b>Supports separated:</b> continue only if the runner\u2019s path improves; do not sacrifice eastward progress to restore a parade formation.')
        p.box('The decisive question','Which single hull can still satisfy both the transit and the two-turn clear count? If the answer changes, the support plan changes with it.')
    else:
        heads=['Phase / trigger','Onondaga','Mahopac','Sangamon','Command / decision']
        rows=[
            ['I / SCREEN\nT1\u201310\n14:00\u201314:30','Central block. Prepare useful arcs for both turrets.','Make contact; do not open a lane behind the contact ship.','Retain a mobile backstop for the uncommitted route.','Name sectors and reserve. Trigger: enemy runner / main lane identified.'],
            ['II / FIX\nT11\u201330\n14:30\u201315:30','Coordinate target allocation and legal gun readiness.','Pressure the runner\u2019s route and compel a turn.','Shift toward the dangerous lane without chasing a feint.','RUNNER / SWITCH as needed. Trigger: enemy commits past the forts.'],
            ['III / CLOSE\nT31\u201350\n15:30\u201316:30','Hold the best intercepting water; avoid friendly masking.','Maintain a legal engagement or hand off with acknowledgement.','Close the neglected exit route; accept reserve commitment when decisive.','Confirm who can stop the next candidate, not only the current one. Trigger: crossing at LIBERTY.'],
            ['IV / DENY\nT51\u201360\n16:30\u201317:00','Direct remaining guns and mobile hulls against qualifying candidates.','Deny clear-count conditions where legally possible.','Backstop a second candidate and cover a failed pursuit.','Resolve all actions before the count. Zero completed escapes at T60 wins.'],
        ]
        p.table(heads,rows,[79,109,109,109,118])
        p.h('Branches / decisions')
        p.p('<b>Split breakout:</b> allocate one initial blocker per threat while retaining cross-support. <b>Reserve disabled:</b> announce HOLE and redraw sectors at once. <b>Candidate across gate:</b> pursue with the best-positioned legal engager; others cover the next hull. <b>Flagship disabled:</b> command succession and target allocation occur together.')
        p.box('The decisive question','Can any Confederate hull obtain two qualifying turns while we are fighting somewhere else? If yes, the division is defending the wrong water.')


def quickref(p,side):
    p.page('09 / Captain\u2019s working card','Bring this page to the table')
    p.h('Before the first orders')
    p.p('Confirm the expanded coastal board, all six ship cards, the 60-turn clock and the marked column-184 gate segment. Identify each captain and the separate umpire. State plainly that one escaping Confederate ironclad wins; no second Confederate survivor is required. Declare baseline shore fire OFF or issue the full active-fort variant before either side plans.')
    p.p('Mark navigable water, any shoals or prohibited hexes, and the agreed visibility / smoke effects on the common chart. Fix ammunition, movement, loading, ramming, damage and repair procedures from the chosen rules. Do not mix Iron & Fire inches with Ironclads hex allowances. Reveal free initial headings together; no free displacement is granted.')
    p.h('Every turn')
    p.table(['Record','Captain / umpire entry'],[
        ['Resolved turn / game time','___________________ / ___________________'],
        ['Assigned ship and current task','________________________________________'],
        ['Permitted movement / steering','________________________________________'],
        ['Ready batteries / targets / legal arcs','________________________________________'],
        ['Current runner / primary interception','________________________________________'],
        ['Candidate gate crossing / clear count','________________________________________'],
        ['Damage, repair and succession changes','________________________________________'],
    ],[191,333])
    p.h('Keep the software in its proper role')
    p.p('The app supplies models, the clock, coordinates, measurements and visual effects. Captains and umpire enforce movement, reloads, legal fire, damage, visibility and victory. Triggering a cannon effect does not prove a legal shot. Passing an eastern map edge does not independently satisfy this packet\u2019s gate and clear-count rules.')
    p.p('Save the table before deploying or switching scenarios. Save again after each five resolved turns and at major gate / damage decisions. The table save does not replace the written ammunition, damage and clear-count ledger. Use CSV position logs if useful. These packets supersede the old two-survivor objective even if an older executable still displays it.')
    p.h('Commander\u2019s final reminder')
    p.box('Intent', 'One ironclad free and clear is enough. Fight for a surviving route, not for an impressive exchange of salvos.' if side=='Confederate' else 'Every Confederate ironclad matters. Deny the escape conditions until the clock expires; a tactical duel is subordinate to containment.')
    p.p('The 60-turn duration and baseline fort abstraction are design choices, not balance results. Human play has not yet established whether either side needs a handicap. Change the contract only between matches and issue the same revision to both sides.','small')


def references(p):
    p.page('10 / Notes & provenance','History, inference & game design')
    p.p('<b>Historical record</b> is cited by H-number. <b>Inference</b> is the operational reasoning connecting survival, blockade pressure and ship roles. <b>Game design</b> comprises the early arrival, successful river passage, extended-war branch, three-versus-three concentration, orders, signals, weather, gate, clear count, turn limit and fort-fire abstraction. These categories are deliberately distinct.')
    for code,title,url,note in SOURCES:
        p.p(f'<b>{code} / <link href="{url}" color="#24607B">{escape(title)}</link></b><br/>{escape(note)}','small')
    p.h('Ship-rating and map notes')
    p.p('The existing module models 19 guns. Its Virginia II forward 7-inch rifle follows the supplied game card; historical summaries differ. Fredericksburg\u2019s modeled 8-inch / XI-inch outfit remains provisional. Use agreed cards, not this packet, to settle numerical ratings. Private ship sheets and rulebook scans are not reproduced. No assertion of an exact 1865 battery inventory or hydrographic survey is made.','small')
    p.p('The tactical diagram is an original schematic. Gate coordinates refer to this project\u2019s expanded 100-yard grid and preserved origin, not latitude / longitude, a government navigation chart or the printed board-game maps. Modern UTC is retained as the app\u2019s clock convention; it is not a claim about a captain\u2019s 1865 timekeeping.','small')
    p.h('Acronyms')
    p.p('<b>METT-TC:</b> Mission; Enemy; Terrain and weather; Troops and support available; Time available; Civil considerations.<br/><b>OCOKA:</b> Observation and fields of fire; Cover and concealment; Obstacles; Key terrain; Avenues of approach.<br/><b>SMEAC:</b> Situation; Mission; Execution; Administration and logistics; Command and signal.','small')
    p.p('Prepared 21 November 2026 for Ironclads Digitized. Original scenario prose and diagram; sources linked above. This is an independent wargame supplement, not a published historical order or an endorsed official scenario. No gameplay balance trials were conducted for these briefing materials. Pagination and visual layout are reviewed separately.','small')


def main():
    for side,accent in [('Confederate','#C29A66'),('Union','#4586A3')]:
        p=Packet(side,accent)
        if side=='Confederate':
            p.cover('BREAKOUT ORDER / HAMPTON ROADS / 28 MARCH 1865',
                    'Get at least one ironclad free and clear.\nThe squadron\u2019s survival is the mission.',
                    'CSS Stonewall \u2022 CSS Virginia II \u2022 CSS Fredericksburg',
                    'Preserve an armored force beyond the James and retain a threat to Union maritime operations in an extended-war campaign.')
        else:
            p.cover('INTERCEPTION ORDER / HAMPTON ROADS / 28 MARCH 1865',
                    'Let no Confederate ironclad escape.\nHold the gate until the deadline.',
                    'USS Onondaga \u2022 USS Mahopac \u2022 USS Sangamon',
                    'Maintain control of the roadstead and prevent a surviving Confederate ironclad from becoming a renewed coastal or ocean threat.')
        common_history(p);common_contract(p);terrain(p,side);mett(p,side)
        situation_mission(p,side);execution(p,side);admin_command(p,side)
        sync(p,side);quickref(p,side);references(p);p.build()


if __name__=='__main__':
    main()
