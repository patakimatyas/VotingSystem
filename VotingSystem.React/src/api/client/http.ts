const BASE_URL = import.meta.env.VITE_APP_API_BASEURL;

export async function get<T>(url: string, params?: Record<string, string>): Promise<T> {
    let fullUrl = `${import.meta.env.VITE_APP_API_BASEURL}/${url}`;
    if (params) {
        const queryString = new URLSearchParams(params).toString();
        fullUrl += `?${queryString}`;
    }

    const res = await fetch(fullUrl, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("authToken")}`,
            "Content-Type": "application/json"
        }
    });

    await throwErrorIfNotOk(res);

    return await res.json();
}


export async function postAsJson<TRequest, TResponse>(
    url: string,
    body: TRequest
): Promise<TResponse> {
    const res = await fetch(`${BASE_URL}/${url}`, {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("authToken")}`,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });

    await throwErrorIfNotOk(res);

    return await res.json();
}
export async function postAsJsonWithoutResponse<TRequest>(
    url: string,
    body?: TRequest
): Promise<void> {
    const res = await fetch(`${BASE_URL}/${url}`, {
        method: "POST",
        body: body ? JSON.stringify(body) : undefined,
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("authToken")}`,
            "Content-Type": "application/json"
        }
    });

    await throwErrorIfNotOk(res);
}

async function throwErrorIfNotOk(res: Response) {
    if (res.ok) return;
    if (res.status === 401) {
        localStorage.removeItem("user");
        localStorage.removeItem("authToken");
        window.location.href = "/users/login";
        return;
    }
    let errorText: string;

    try {
        errorText = await res.text();
    } catch {
        errorText = `${res.status} ${res.statusText}`;
    }

    throw new Error(errorText);
}
