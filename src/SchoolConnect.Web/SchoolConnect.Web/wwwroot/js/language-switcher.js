(() => {
    const storageKey = "schoolconnect-language";
    const originals = new WeakMap();
    let language = localStorage.getItem(storageKey) === "te" ? "te" : "en";
    let translating = false;

    const phrases = {
        "Sri Venkateswara Convent": "శ్రీ వెంకటేశ్వర కాన్వెంట్",
        "Bayyavaram, Krosur Mandal, Palnadu District, Andhra Pradesh, India": "బయ్యవరం, క్రోసూరు మండలం, పల్నాడు జిల్లా, ఆంధ్రప్రదేశ్, భారతదేశం",
        "A connected school experience for students, teachers, parents, and administrators with academics, attendance, notices, transport, and management workflows.": "విద్య, హాజరు, ప్రకటనలు, రవాణా మరియు నిర్వహణ సేవలతో విద్యార్థులు, ఉపాధ్యాయులు, తల్లిదండ్రులు, నిర్వాహకులకు అనుసంధానమైన పాఠశాల అనుభవం.",
        "Teaching excellence since 1986": "1986 నుండి బోధనలో శ్రేష్ఠత",
        "Excellence in Teaching": "బోధనలో శ్రేష్ఠత",
        "35+ years of trusted education": "35+ సంవత్సరాల విశ్వసనీయ విద్య",
        "Bright spaces for learning": "నేర్చుకోవడానికి ఆహ్లాదకరమైన ప్రదేశాలు",
        "Modern classrooms and a clean campus create a better daily school experience.": "ఆధునిక తరగతి గదులు, పరిశుభ్రమైన ప్రాంగణం మెరుగైన పాఠశాల అనుభవాన్ని అందిస్తాయి.",
        "Interactive teaching moments": "పరస్పర బోధనా అనుభవాలు",
        "Students learn in structured sessions with attentive classroom engagement.": "విద్యార్థులు శ్రద్ధతో కూడిన క్రమబద్ధమైన తరగతుల్లో నేర్చుకుంటారు.",
        "Sports and activity time": "క్రీడలు మరియు కార్యకలాపాల సమయం",
        "Balanced growth comes from academics, movement, and student activities.": "విద్య, వ్యాయామం, విద్యార్థి కార్యకలాపాలు సమతుల్య అభివృద్ధిని అందిస్తాయి.",
        "Designed for a complete digital school experience": "సంపూర్ణ డిజిటల్ పాఠశాల అనుభవం కోసం రూపొందించబడింది",
        "Curriculum-first planning": "పాఠ్య ప్రణాళికకు ప్రాధాన్యం",
        "Class-wise timetable, syllabus, lesson units, worksheets, and exam preparation content.": "తరగతి వారీ సమయపట్టిక, సిలబస్, పాఠాలు, వర్క్‌షీట్లు మరియు పరీక్ష సన్నాహక సమాచారం.",
        "Notices and feedback": "ప్రకటనలు మరియు అభిప్రాయం",
        "Important circulars, teacher remarks, academic updates, and parent-visible progress.": "ముఖ్యమైన సర్క్యులర్లు, ఉపాధ్యాయుల వ్యాఖ్యలు, విద్యా నవీకరణలు మరియు తల్లిదండ్రులకు కనిపించే పురోగతి.",
        "School management tools": "పాఠశాల నిర్వహణ సాధనాలు",
        "Teacher workflows for attendance, reports, events, transport coordination, and maintenance.": "హాజరు, నివేదికలు, కార్యక్రమాలు, రవాణా సమన్వయం మరియు నిర్వహణ కోసం ఉపాధ్యాయ సాధనాలు.",
        "Student progress view": "విద్యార్థి పురోగతి వీక్షణ",
        "Attendance percentage, assessment summaries, homework status, and academic remarks.": "హాజరు శాతం, మూల్యాంకన సారాంశాలు, హోంవర్క్ స్థితి మరియు విద్యా వ్యాఖ్యలు.",
        "News & Announcements": "వార్తలు & ప్రకటనలు",
        "Reach the school team": "పాఠశాల బృందాన్ని సంప్రదించండి",
        "Call during school hours for admissions, transport, and office support.": "ప్రవేశాలు, రవాణా మరియు కార్యాలయ సహాయం కోసం పాఠశాల వేళల్లో కాల్ చేయండి.",
        "Direct contacts for school coordination and academic support.": "పాఠశాల సమన్వయం మరియు విద్యా సహాయం కోసం ప్రత్యక్ష సంప్రదింపులు.",
        "Welcome to Our School": "మా పాఠశాలకు స్వాగతం",
        "Why Choose Our School?": "మా పాఠశాలను ఎందుకు ఎంచుకోవాలి?",
        "What we stand for": "మా విలువలు",
        "Back to dashboard": "డాష్‌బోర్డ్‌కు తిరిగి వెళ్ళండి",
        "Browse old student memories grouped by passed-out year.": "ఉత్తీర్ణత సంవత్సరం వారీగా పూర్వ విద్యార్థుల జ్ఞాపకాలను చూడండి.",
        "No gallery photos yet": "ఇంకా గ్యాలరీ ఫోటోలు లేవు",
        "Select your login type and enter your PIN and password.": "మీ లాగిన్ రకాన్ని ఎంచుకొని పిన్ మరియు పాస్‌వర్డ్ నమోదు చేయండి.",
        "You are already logged in": "మీరు ఇప్పటికే లాగిన్ అయ్యారు",
        "PIN or password does not match the selected login type.": "ఎంచుకున్న లాగిన్ రకానికి పిన్ లేదా పాస్‌వర్డ్ సరిపోలలేదు.",
        "Check your password and try again.": "మీ పాస్‌వర్డ్ తనిఖీ చేసి మళ్లీ ప్రయత్నించండి.",
        "PIN is required.": "పిన్ తప్పనిసరి.",
        "Password is required.": "పాస్‌వర్డ్ తప్పనిసరి.",
        "Home": "హోమ్", "Gallery": "గ్యాలరీ", "Why Choose Us": "మమ్మల్ని ఎందుకు ఎంచుకోవాలి",
        "Latest Updates": "తాజా సమాచారం", "Admissions": "ప్రవేశాలు", "About Us": "మా గురించి",
        "Contact Us": "సంప్రదించండి", "Student Login": "విద్యార్థి లాగిన్", "Teacher Login": "ఉపాధ్యాయ లాగిన్",
        "Login": "లాగిన్", "Profile": "ప్రొఫైల్", "Profile details": "ప్రొఫైల్ వివరాలు",
        "Profile overview": "ప్రొఫైల్ అవలోకనం", "Open Portal": "పోర్టల్ తెరవండి", "Sign out": "లాగ్ అవుట్",
        "Active": "సక్రియం", "Student name": "విద్యార్థి పేరు", "Teacher name": "ఉపాధ్యాయుని పేరు",
        "Class": "తరగతి", "Gender": "లింగం", "Parents / Guardian name": "తల్లిదండ్రులు / సంరక్షకుని పేరు",
        "Parent / guardian": "తల్లిదండ్రులు / సంరక్షకులు", "Parent / guardian name": "తల్లిదండ్రులు / సంరక్షకుని పేరు",
        "Mobile phone number": "మొబైల్ ఫోన్ నంబర్", "Mobile number": "మొబైల్ నంబర్",
        "Class teacher": "తరగతి ఉపాధ్యాయుడు", "School joined year": "పాఠశాలలో చేరిన సంవత్సరం",
        "Class dealing with": "బోధిస్తున్న తరగతి", "Subject": "విషయం", "Qualification": "అర్హత",
        "Campus": "ప్రాంగణం", "Classroom": "తరగతి గది", "Activity": "కార్యకలాపం",
        "Academics": "విద్య", "Communication": "సమాచార మార్పిడి", "Operations": "నిర్వహణ", "Growth": "అభివృద్ధి",
        "Live Updates": "ప్రత్యక్ష సమాచారం", "View All Notices": "అన్ని ప్రకటనలు చూడండి",
        "Start Enquiry": "విచారణ ప్రారంభించండి", "Admission Form": "ప్రవేశ దరఖాస్తు",
        "Admission enquiry": "ప్రవేశ విచారణ", "Quick enquiry": "త్వరిత విచారణ",
        "Enquiry submitted.": "విచారణ సమర్పించబడింది.", "Select class": "తరగతిని ఎంచుకోండి",
        "Email address": "ఈమెయిల్ చిరునామా", "Enquiry details": "విచారణ వివరాలు",
        "Submit enquiry": "విచారణ సమర్పించండి", "Read About Us": "మా గురించి చదవండి",
        "Our Mission": "మా లక్ష్యం", "Our Vision": "మా దార్శనికత", "Our Commitment": "మా నిబద్ధత",
        "Find Campus": "ప్రాంగణాన్ని కనుగొనండి", "School Email": "పాఠశాల ఈమెయిల్",
        "General Office": "ప్రధాన కార్యాలయం", "Phone": "ఫోన్", "Faculty Contacts": "ఉపాధ్యాయుల సంప్రదింపులు",
        "Administration & Support": "పరిపాలన & సహాయం", "Login to portal": "పోర్టల్‌లో లాగిన్ అవ్వండి",
        "Access denied": "అనుమతి లేదు", "Teacher login unavailable": "ఉపాధ్యాయ లాగిన్ అందుబాటులో లేదు",
        "Teacher portal unavailable": "ఉపాధ్యాయ పోర్టల్ అందుబాటులో లేదు", "Password": "పాస్‌వర్డ్",
        "Continue": "కొనసాగించండి", "Cancel": "రద్దు", "Close": "మూసివేయండి", "Reload": "మళ్లీ లోడ్ చేయండి",
        "Apply for admission": "ప్రవేశానికి దరఖాస్తు చేయండి", "Admission application": "ప్రవేశ దరఖాస్తు",
        "Online application": "ఆన్‌లైన్ దరఖాస్తు", "Application submitted.": "దరఖాస్తు సమర్పించబడింది.",
        "Student full name": "విద్యార్థి పూర్తి పేరు", "Date of birth": "పుట్టిన తేదీ",
        "Select gender": "లింగాన్ని ఎంచుకోండి", "Female": "స్త్రీ", "Male": "పురుషుడు", "Other": "ఇతర",
        "Class applying for": "దరఖాస్తు చేస్తున్న తరగతి", "Relationship to student": "విద్యార్థితో సంబంధం",
        "Select relationship": "సంబంధాన్ని ఎంచుకోండి", "Father": "తండ్రి", "Mother": "తల్లి", "Guardian": "సంరక్షకుడు",
        "Address": "చిరునామా", "Submit application": "దరఖాస్తు సమర్పించండి", "Notices": "ప్రకటనలు",
        "Timetable": "సమయపట్టిక", "Curriculum": "పాఠ్య ప్రణాళిక", "Attendance": "హాజరు",
        "Assignments": "అసైన్‌మెంట్లు", "Fees": "ఫీజులు", "Transport": "రవాణా",
        "Dashboard": "డాష్‌బోర్డ్", "Details": "వివరాలు", "Status": "స్థితి", "Priority": "ప్రాధాన్యత",
        "High": "అధిక", "Medium": "మధ్యస్థ", "Low": "తక్కువ", "Today": "ఈ రోజు"
    };

    const entries = Object.entries(phrases).sort((a, b) => b[0].length - a[0].length);
    const ignored = new Set(["SCRIPT", "STYLE", "CODE", "PRE", "TEXTAREA"]);

    function translateText(value) {
        let result = value;
        for (const [english, telugu] of entries) result = result.split(english).join(telugu);
        return result;
    }

    function translateNode(node) {
        if (node.nodeType === Node.TEXT_NODE) {
            if (!node.nodeValue?.trim() || ignored.has(node.parentElement?.tagName)) return;
            if (!originals.has(node)) originals.set(node, node.nodeValue);
            node.nodeValue = language === "te" ? translateText(originals.get(node)) : originals.get(node);
            return;
        }
        if (!(node instanceof Element)) return;
        if (node.closest("[data-no-translate]") || ignored.has(node.tagName)) return;
        for (const attribute of ["aria-label", "title", "placeholder"]) {
            if (!node.hasAttribute(attribute)) continue;
            const key = `languageOriginal${attribute.replace(/-([a-z])/g, (_, c) => c.toUpperCase())}`;
            node.dataset[key] ??= node.getAttribute(attribute);
            node.setAttribute(attribute, language === "te" ? translateText(node.dataset[key]) : node.dataset[key]);
        }
        for (const child of node.childNodes) translateNode(child);
    }

    function updateToggle() {
        const button = document.getElementById("language-toggle");
        if (!button) return;
        const label = button.querySelector("[data-language-label]");
        const desiredLabel = language === "te" ? "English" : "తెలుగు";
        const desiredTitle = language === "te" ? "Switch to English" : "తెలుగుకు మార్చండి";
        if (label.textContent !== desiredLabel) label.textContent = desiredLabel;
        if (button.getAttribute("aria-label") !== desiredTitle) button.setAttribute("aria-label", desiredTitle);
        if (button.getAttribute("title") !== desiredTitle) button.setAttribute("title", desiredTitle);
    }

    function apply() {
        translating = true;
        document.documentElement.lang = language;
        translateNode(document.body);
        updateToggle();
        translating = false;
    }

    document.addEventListener("click", event => {
        if (!event.target.closest("#language-toggle")) return;
        language = language === "en" ? "te" : "en";
        localStorage.setItem(storageKey, language);
        apply();
    });

    new MutationObserver(mutations => {
        if (translating || language !== "te") return;
        translating = true;
        for (const mutation of mutations) {
            for (const node of mutation.addedNodes) translateNode(node);
        }
        updateToggle();
        translating = false;
    }).observe(document.documentElement, { childList: true, subtree: true });

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", apply);
    else apply();
})();
