export default interface NavigationItem {
    name: string;
    url: string;

    children: NavigationItem[]
}