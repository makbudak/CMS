import axios from 'axios';
import authHeader from "./AuthHeader";
import { API_URL } from "./Endpoints"; 

export class GlobalService {

    AddApiUrl(endPoint) {
        return API_URL + endPoint;
    }

    Get(url) {
        return axios.get(this.AddApiUrl(url));
    }

    GetByAuth(url) {
        return axios.get(this.AddApiUrl(url), {
            headers: authHeader()
        });
    }

    Post(url, data) {
        return axios.post(this.AddApiUrl(url), data);
    }

    PostByAuth(url, data) {
        return axios.post(this.AddApiUrl(url), data, {
            headers: authHeader()
        });
    }

    Put(url, data) {
        return axios.put(this.AddApiUrl(url), data);
    }

    PutByAuth(url, data) {
        return axios.put(this.AddApiUrl(url), data, {
            headers: authHeader()
        });
    }

    Delete(url, id) {
        return axios.delete(this.AddApiUrl(url) + "/" + id);
    }

    DeleteByAuth(url, id) {
        return axios.delete(this.AddApiUrl(url) + "/" + id, {
            headers: authHeader()
        });
    }
}
export default new GlobalService();