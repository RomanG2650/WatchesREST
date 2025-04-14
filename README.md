sikkerhed som en integreret del af systemet – ikke bare som et teknisk krav, men som en del af den samlede brugeroplevelse.

Alle nødvendige sikkerhedsforanstaltninger er implementeret, herunder:

- JWT-login med rollebaseret adgangskontrol (RBAC)

- Rate limiting ved login: 3 mislykkede forsøg → konto låses i 10 minutter

- Inputvalidering og beskyttelse mod SQL injection

- Brugeren informeres ved mistænkelig aktivitet

- Cross-Site Scripting (XSS)-beskyttelse via korrekt output encoding og sanitization

- Content Security Policy (CSP) for at begrænse kørsel af uautoriseret scripts og ressourcer

- HTTP Headers som X-Content-Type-Options, X-Frame-Options, og Strict-Transport-Security er sat for at forhindre clickjacking og sikre korrekt indholdsbehandling

- HTTPS anvendes til al kommunikation for at beskytte data i transit

Projektet sigter mod at være en komplet og sikker backend-løsning til en professionel webshop med fokus på ure.

