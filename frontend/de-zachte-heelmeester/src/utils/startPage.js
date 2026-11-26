// Which start page belongs to which role (role names as in DbSeeder)
const startPages = {
    Patient: 'patient-start',
    Huisarts: 'general-practitioner-start',
    Specialist: 'specialist-start',
    Medewerker: 'employee-start',
    Beheerder: 'admin-start',
}

export function startPageFor(roles) {
    return startPages[roles[0]] ?? 'login'
}